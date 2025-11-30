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
/api/{resources}?version={version}
```

### Règles strictes

1. **Pas de trailing forward slash** : ❌ `/api/users/` → ✅ `/api/users`
2. **Kebab-case (lower-kebab-case)** : Utiliser des tirets pour séparer les mots
   - ✅ `/api/user-profiles`
   - ❌ `/api/userProfiles` ou `/api/user_profiles`
3. **Pas d'action dans l'endpoint** : Le verbe HTTP définit l'action
   - ❌ `/api/users/get` ou `/api/users/create`
   - ✅ `GET /api/users` ou `POST /api/users`
4. **Toujours au pluriel** : Les ressources sont toujours au pluriel
   - ✅ `/api/users`, `/api/orders`, `/api/products`
   - ❌ `/api/user`, `/api/order`
5. **Éviter la complexité** : Pas de nesting profond
   - ❌ `/api/customers/2/stores/31/orders/153/items/active`
   - ✅ `/api/orders/153/items/active` (si on connaît l'ID de la commande)

### Exemples corrects
- `GET /api/users?version=v1`
- `POST /api/users?version=v2`
- `GET /api/user-profiles?version=v1`
- `PUT /api/orders/123?version=v2`
- `GET /api/orders/123/items?version=v1`
- `DELETE /api/products/456?version=v1`

### Exemples incorrects
- ❌ `GET /api/user/get?version=v1` (action dans l'URL)
- ❌ `POST /api/user/create?version=v2` (action dans l'URL)
- ❌ `GET /api/users/?version=v1` (trailing slash)
- ❌ `GET /api/v3/users` (la version est un pathVariable)
- ❌ `GET /api/userProfiles?version=v1` (camelCase au lieu de kebab-case)
- ❌ `GET /api/user?version=v1` (singulier au lieu de pluriel)

### Gestion de la version

1. **Version dans query string** : `?version=v1`
2. **Version par défaut** : Si `version` est absent, utiliser `ApiSettings.DefaultVersion` depuis `appsettings.json`
3. **Version requise** : Tous les endpoints doivent accepter le paramètre `version`
4. **Passage de version** : La version est passée aux handlers via le contexte ou comme paramètre

### Structure Endpoints.cs

```csharp
public static class FeatureNameEndpoints
{
    public static void MapFeatureNameEndpoints(this WebApplication app, IConfiguration configuration)
    {
        var group = app.MapGroup("/api/resources")
            .WithTags("Resources");

        group.MapGet("/", HandleGetAll)
            .WithName("GetResources")
            .Produces<ResourceResponse>(200)
            .Produces<ErrorResponse>(400);

        group.MapGet("/{id}", HandleGetById)
            .WithName("GetResourceById")
            .Produces<ResourceResponse>(200)
            .Produces<ErrorResponse>(404);

        group.MapPost("/", HandleCreate)
            .WithName("PostResources")
            .Accepts<CreateResourceRequest>("application/json")
            .Produces<ResourceResponse>(200)
            .Produces<ErrorResponse>(400);
    }

    private static Task<IResult> HandleGetAll(
        string? version,
        IConfiguration configuration)
    {
        var traceId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var apiVersion = string.IsNullOrEmpty(version) 
            ? GetDefaultVersion(configuration) 
            : version;

        return ErrorHandlingExtensions.HandleAsync(async () =>
        {
            var result = ResourceHandler.GetAll(apiVersion);
            return await Task.FromResult(result);
        }, traceId);
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

### Program.cs doit rester très clean

Le fichier `Program.cs` doit être minimal et lisible :

```csharp
using MinimalAPI.Configuration;
using MinimalAPI.Features.Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.UseSwaggerConfiguration();

app.MapGet("/", () => "Hello World!");

app.MapTestEndpoints(app.Configuration);

app.Run();
```

**Règles** :
- Pas de configuration complexe dans `Program.cs`
- Swagger dans `Configuration/SwaggerConfiguration.cs`
- Tous les endpoints dans leurs fichiers `Endpoints.cs` respectifs
- Maximum 15-20 lignes dans `Program.cs`

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
- **Configuration** : Configuration Swagger, services, etc. dans `Configuration/`

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

## Formatage du code

### Outil de formatage

Le projet utilise **`dotnet format`** pour maintenir un style de code cohérent.

### Commandes

```bash
# Formater tous les fichiers du projet
dotnet format MinimalAPI.sln

# Formater uniquement le projet principal
dotnet format MinimalAPI.csproj

# Vérifier le formatage sans modifier (dry-run)
dotnet format MinimalAPI.sln --verify-no-changes
```

### Règles de formatage

- **Formatage automatique** : Exécuter `dotnet format` avant chaque commit
- **Vérification CI/CD** : Intégrer `dotnet format --verify-no-changes` dans le pipeline
- **Style cohérent** : Respecter les conventions C# standard (.editorconfig)

### Workflow recommandé

1. Faire les modifications de code
2. Exécuter `dotnet format MinimalAPI.sln`
3. Vérifier les changements avec `git diff`
4. Commiter les changements de formatage séparément si nécessaire

### Configuration

Le formatage suit les règles définies dans :
- **`.editorconfig`** : Fichier de configuration à la racine du projet
  - Conventions C# modernes
  - Règles de nommage
  - Préférences d'indentation et d'espacement
  - Utilisé automatiquement par `dotnet format` et les IDE
- Conventions C# par défaut de .NET (si non spécifié dans `.editorconfig`)

Le fichier `.editorconfig` est **obligatoire** et doit être présent à la racine du projet.

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
- [ ] Le code a été formaté avec `dotnet format`

---

## Gestion d'erreurs structurées

### Principe

**Jamais de plain error 500** : Toujours retourner des erreurs structurées avec des informations utiles.

### ErrorResponse

Utiliser le type `ErrorResponse` pour toutes les erreurs :

```csharp
public record ErrorResponse
{
    public string ErrorCode { get; init; }
    public string Message { get; init; }
    public string? Details { get; init; }
    public DateTime Timestamp { get; init; }
    public string? TraceId { get; init; }
}
```

### Utilisation

```csharp
// Erreur de validation
var error = ErrorResponse.ValidationError("Email is required");

// Erreur not found
var error = ErrorResponse.NotFound("User", userId);

// Erreur interne avec traceId
var error = ErrorResponse.InternalError("Database connection failed", traceId);
```

### ErrorHandlingExtensions

Utiliser les extensions pour gérer les erreurs automatiquement :

```csharp
return ErrorHandlingExtensions.HandleAsync(async () =>
{
    var result = Handler.Handle(request);
    return await Task.FromResult(result);
}, traceId);
```

### Règles

- **Toujours un traceId** : Pour pouvoir tracer les erreurs en production
- **Codes d'erreur explicites** : `VALIDATION_ERROR`, `NOT_FOUND`, `INTERNAL_ERROR`
- **Messages clairs** : Expliquer ce qui s'est passé
- **Pas de stack trace** : En production, seulement en développement
- **Tests obligatoires** : Tester tous les cas d'erreur

---

## Notes importantes

- Ce ruleset doit être suivi **à la lettre**
- Toute déviation doit être discutée et documentée
- Les tests sont **obligatoires** avant toute merge
- La programmation fonctionnelle est **non négociable**
- Le formatage du code avec `dotnet format` est **obligatoire** avant chaque commit
- **Program.cs doit rester très clean** : Maximum 15-20 lignes
- **Pas de plain error 500** : Toujours des erreurs structurées

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

