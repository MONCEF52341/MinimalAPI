using Microsoft.OpenApi.Models;
using MinimalAPI.Features.Test;

var builder = WebApplication.CreateBuilder(args);

// Ajout des services Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Minimal API - Functional Programming",
    Version = "v1",
    Description = "API Minimal avec architecture fonctionnelle et pattern Features. " +
                   "Tous les endpoints acceptent un paramètre optionnel 'version' dans la query string. " +
                   "Si non spécifié, la version par défaut (v1) sera utilisée."
  });
  
  // Ajout du paramètre version global pour tous les endpoints
  c.AddServer(new OpenApiServer
  {
    Url = "/",
    Description = "Serveur principal"
  });
});

var app = builder.Build();

// Configuration Swagger (uniquement en développement)
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API v1");
    c.RoutePrefix = "swagger";
  });
}

app.MapGet("/", () => "Hello World!");

// Enregistrement des endpoints de features
app.MapTestEndpoints(app.Configuration);

app.Run();
