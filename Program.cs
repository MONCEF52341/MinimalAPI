using MinimalAPI.Configuration;
using MinimalAPI.Features.Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.UseSwaggerConfiguration();

app.MapGet("/", () => "Hello World!");

app.MapTestEndpoints(app.Configuration);

app.Run();
