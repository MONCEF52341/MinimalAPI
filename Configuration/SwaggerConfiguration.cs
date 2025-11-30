using Microsoft.OpenApi.Models;

namespace MinimalAPI.Configuration;

/// <summary>
/// Configuration Swagger/OpenAPI pour l'application
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configure les services Swagger
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Minimal API - Functional Programming",
                Version = "v1",
                Description = "API Minimal avec architecture fonctionnelle et pattern Features. " +
                             "Tous les endpoints acceptent un paramètre optionnel 'version' dans la query string. " +
                             "Si non spécifié, la version par défaut (v1) sera utilisée."
            });

            c.AddServer(new OpenApiServer
            {
                Url = "/",
                Description = "Serveur principal"
            });
        });

        return services;
    }

    /// <summary>
    /// Configure le middleware Swagger
    /// </summary>
    public static WebApplication UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API v1");
                c.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}

