var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGet("/api/test", (string version) => $"DEMO Hello World! Version = {version}");

app.Run();
