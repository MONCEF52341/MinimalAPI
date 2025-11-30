# Ruleset - Architecture Fonctionnelle .NET Minimal API

## 📋 Table des matières

1. [Principes fondamentaux](#principes-fondamentaux)
2. [Architecture Features](#architecture-features)
3. [Conventions de nommage](#conventions-de-nommage)
4. [Pattern d'endpoints](#pattern-dendpoints)
5. [Programmation fonctionnelle](#programmation-fonctionnelle)
6. [Tests unitaires](#tests-unitaires)
7. [Organisation du code](#organisation-du-code)
8. [Documentation Swagger/OpenAPI](#documentation-swaggeropenapi)

---

## Principes fondamentaux

### Philosophie
Ce projet suit une approche **100% programmation fonctionnelle** :
- ❌ **PAS** de classes de service traditionnelles
- ❌ **PAS** de structure MVC avec Controllers globaux
- ✅ **OUI** aux fonctions pures
- ✅ **OUI** à l'immutabilité
- ✅ **OUI** à la composition de fonctions
- ✅ **OUI** aux types immutables (records C#)

### Règles d'or
1. **Immutabilité** : Tous les modèles sont des `record` immutables
2. **Fonctions pures** : Pas d'effets de bord, résultats prévisibles
3. **Séparation des responsabilités** : Chaque fonction a une seule responsabilité
4. **Testabilité** : Chaque fonction pure doit être testable isolément

---

## Architecture Features

### Structure des dossiers

Chaque feature est organisée dans son propre dossier sous `Features/` :

```
Features/
├── FeatureName/
│   ├── Handlers/          # Fonctions pures pour la logique métier
│   │   └── FeatureNameHandler.cs
│   ├── Models/            # Types immutables (records)
│   │   ├── Request.cs
│   │   ├── Response.cs
│   │   └── Domain.cs
│   ├── Validators/        # Fonctions de validation
│   │   └── FeatureNameValidator.cs
│   └── Endpoints.cs       # Définition des routes Minimal API
```

### Exemple de structure complète

```
Features/
├── UserManagement/
│   ├── Handlers/
│   │   ├── CreateUserHandler.cs
│   │   ├── GetUserHandler.cs
│   │   └── UpdateUserHandler.cs
│   ├── Models/
│   │   ├── CreateUserRequest.cs
│   │   ├── UserResponse.cs
│   │   └── User.cs
│   ├── Validators/
│   │   └── UserValidator.cs
│   └── Endpoints.cs
└── ProductCatalog/
    ├── Handlers/
    ├── Models/
    ├── Validators/
    └── Endpoints.cs
```

### Règles de structure
- **Une feature = un domaine métier** (UserManagement, ProductCatalog, OrderProcessing, etc.)
- **Pas de dépendances entre features** : Chaque feature est indépendante
- **Communication entre features** : Via des interfaces/types partagés dans un dossier `Shared/` si nécessaire

---

## Conventions de nommage

### Features
- **Nom du dossier** : PascalCase (ex: `UserManagement`, `ProductCatalog`)
- **Nom du fichier Endpoints** : Toujours `Endpoints.cs`

### Handlers
- **Nom du fichier** : `{Action}{Entity}Handler.cs` (ex: `CreateUserHandler.cs`, `GetProductHandler.cs`)
- **Nom de la fonction** : `Handle{Action}{Entity}` (ex: `HandleCreateUser`, `HandleGetProduct`)
- **Signature** : Fonction pure qui retourne `Result<T>` ou `Task<Result<T>>`

### Models
- **Request** : `{Action}{Entity}Request.cs` (ex: `CreateUserRequest.cs`)
- **Response** : `{Entity}Response.cs` (ex: `UserResponse.cs`)
- **Domain** : `{Entity}.cs` (ex: `User.cs`)
- **Tous les modèles sont des `record`** : Immutables par défaut

### Validators
- **Nom du fichier** : `{Entity}Validator.cs` (ex: `UserValidator.cs`)
- **Nom de la fonction** : `Validate{Entity}` (ex: `ValidateUser`)
- **Retour** : `ValidationResult` ou `Result<T>`

### Tests
- **Structure parallèle** : `Tests/Features/{FeatureName}/`
- **Nom du fichier** : `{HandlerName}Tests.cs` (ex: `CreateUserHandlerTests.cs`)
- **Nom de la classe** : `{HandlerName}Tests`
- **Nom des méthodes** : `{MethodName}_{Scenario}_Should_{ExpectedResult}`

---

## Pattern d'endpoints

### Format des routes

Tous les endpoints suivent le pattern :
```
/api/{feature}/{action}?version={version}
```

### Exemples
- `GET /api/user/get?version=v1`
- `POST /api/user/create?version=v2`
- `GET /api/product/list?version=v1`
- `PUT /api/order/update?version=v2`

### Gestion de la version

1. **Version dans query string** : `?version=v1`
2. **Version par défaut** : Si `version` est absent, utiliser `ApiSettings.DefaultVersion` depuis `appsettings.json`
3. **Version requise** : Tous les endpoints doivent accepter le paramètre `version`
4. **Passage de version** : La version est passée aux handlers via le contexte ou comme paramètre

### Structure Endpoints.cs

```csharp
public static class FeatureNameEndpoints
{
    public static void MapFeatureNameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/featurename")
            .WithTags("FeatureName");

        group.MapGet("/action", HandleAction)
            .WithName("GetAction")
            .Produces<ResponseType>(200);
    }

    private static async Task<IResult> HandleAction(
        string version,
        [AsParameters] Request request)
    {
        // Validation de la version
        var apiVersion = string.IsNullOrEmpty(version) 
            ? GetDefaultVersion() 
            : version;

        // Appel du handler
        var result = await Handler.Handle(request, apiVersion);
        
        return result.Match(
            success => Results.Ok(success),
            error => Results.BadRequest(error)
        );
    }
}
```

---

## Programmation fonctionnelle

### Pattern Result<T>

Utiliser un pattern `Result<T>` pour gérer les erreurs sans exceptions :

```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}
```

### Fonctions pures

**✅ BON** :
```csharp
public static Result<User> CreateUser(CreateUserRequest request)
{
    if (string.IsNullOrEmpty(request.Email))
        return Result<User>.Failure("Email is required");
    
    var user = new User(request.Email, request.Name);
    return Result<User>.Success(user);
}
```

**❌ MAUVAIS** :
```csharp
public class UserService
{
    private readonly ILogger _logger;
    
    public User CreateUser(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user"); // Effet de bord
        // ...
    }
}
```

### Immutabilité

**✅ BON** - Utiliser des `record` :
```csharp
public record User(string Email, string Name, DateTime CreatedAt);
```

**❌ MAUVAIS** - Classes mutables :
```csharp
public class User
{
    public string Email { get; set; }
    public string Name { get; set; }
}
```

### Composition de fonctions

**✅ BON** :
```csharp
public static Result<UserResponse> HandleCreateUser(CreateUserRequest request)
{
    return ValidateRequest(request)
        .Bind(ValidateEmail)
        .Bind(CreateUserEntity)
        .Map(ToResponse);
}
```

---

## Tests unitaires

### Structure des tests

Les tests suivent la même structure que les features :
```
Tests/
└── Features/
    └── FeatureName/
        ├── Handlers/
        │   └── CreateUserHandlerTests.cs
        ├── Validators/
        │   └── UserValidatorTests.cs
        └── Models/
            └── UserTests.cs
```

### Coverage requis

- **Minimum 80% de coverage** pour toutes les features
- **100% de coverage** pour les handlers critiques
- **Tous les edge cases** doivent être testés

### Exemple de test

```csharp
public class CreateUserHandlerTests
{
    [Fact]
    public void HandleCreateUser_ValidRequest_Should_ReturnSuccess()
    {
        // Arrange
        var request = new CreateUserRequest("test@example.com", "Test User");
        
        // Act
        var result = CreateUserHandler.Handle(request);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void HandleCreateUser_InvalidEmail_Should_ReturnFailure()
    {
        // Arrange
        var request = new CreateUserRequest("invalid-email", "Test User");
        
        // Act
        var result = CreateUserHandler.Handle(request);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("email");
    }
}
```

### Règles de test

1. **Un test par fonction pure** : Chaque handler/validator doit avoir ses tests
2. **Tests d'intégration** : Pour les endpoints complets
3. **Tests de validation** : Pour tous les validators
4. **Tests de edge cases** : Cas limites, valeurs nulles, chaînes vides, etc.
5. **Utiliser FluentAssertions** : Pour des assertions plus lisibles

### Configuration des tests

Le fichier `.csproj` de test doit inclure :
```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
  <PackageReference Include="FluentAssertions" Version="7.0.0" />
</ItemGroup>
```

---

## Organisation du code

### Ordre des éléments dans un fichier

1. **Usings** (si pas d'ImplicitUsings)
2. **Namespace**
3. **Types de domaine** (records, enums)
4. **Fonctions publiques**
5. **Fonctions privées/helpers**

### Séparation des responsabilités

- **Handlers** : Logique métier pure, pas d'I/O
- **Validators** : Validation des données d'entrée
- **Models** : Types de données immutables
- **Endpoints** : Point d'entrée HTTP, orchestration, pas de logique métier

### Flux de données

```
HTTP Request
    ↓
Endpoints.cs (extraction paramètres, validation version)
    ↓
Validator (validation des données)
    ↓
Handler (logique métier pure)
    ↓
Result<T>
    ↓
Endpoints.cs (transformation en réponse HTTP)
    ↓
HTTP Response
```

### Règles d'import

- **Pas de dépendances circulaires** entre features
- **Types partagés** dans `Shared/` si nécessaire
- **Pas de dépendances vers l'infrastructure** dans les handlers (sauf via injection de fonctions)

---

## Checklist de conformité

Avant de créer une nouvelle feature, vérifier :

- [ ] La feature est dans `Features/{FeatureName}/`
- [ ] Tous les modèles sont des `record` immutables
- [ ] Les handlers sont des fonctions pures
- [ ] Les endpoints suivent le pattern `/api/{feature}/{action}?version=x`
- [ ] La version par défaut est gérée si absente
- [ ] Les tests couvrent au moins 80% du code
- [ ] Tous les edge cases sont testés
- [ ] Le code suit les conventions de nommage
- [ ] Pas d'effets de bord dans les handlers
- [ ] Utilisation du pattern `Result<T>` pour les erreurs

---

## Notes importantes

- Ce ruleset doit être suivi **à la lettre**
- Toute déviation doit être discutée et documentée
- Les tests sont **obligatoires** avant toute merge
- La programmation fonctionnelle est **non négociable**

---

## Documentation Swagger/OpenAPI

### Configuration

Swagger/OpenAPI est configuré dans `Program.cs` et activé uniquement en environnement de développement.

### Accès à Swagger

- **URL Swagger UI** : `http://localhost:5000/swagger` (ou le port configuré)
- **URL Swagger JSON** : `http://localhost:5000/swagger/v1/swagger.json`

### Documentation des endpoints

Chaque endpoint doit être documenté avec :
- `.WithSummary()` : Résumé court de l'endpoint
- `.WithDescription()` : Description détaillée incluant les paramètres optionnels
- `.Produces<T>()` : Types de réponse possibles
- `.WithTags()` : Groupe auquel appartient l'endpoint (via `MapGroup`)

### Exemple

```csharp
group.MapGet("/hello", HandleHello)
    .WithName("GetTestHello")
    .WithSummary("Endpoint de test Hello")
    .WithDescription("Retourne un message de test avec la version de l'API. Paramètre optionnel: ?version=v1")
    .Produces<TestResponse>(200)
    .Produces(400);
```

### Règles

- Tous les endpoints doivent être documentés dans Swagger
- Les paramètres de version doivent être mentionnés dans la description
- Les types de réponse (200, 400, etc.) doivent être déclarés
- Les groupes d'endpoints doivent utiliser `.WithTags()` pour l'organisation

---

*Dernière mise à jour : 2024*

