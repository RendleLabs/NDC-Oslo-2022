using Grpc.HealthCheck;
using Ingredients;
using Ingredients.Data;
using Ingredients.Services;
using JaegerTracing;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var runningInContainer = "true".Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));
var macOS = OperatingSystem.IsMacOS();

var builder = WebApplication.CreateBuilder(args);

builder.AddJaegerTracing();

if (runningInContainer)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http2);
    });
}
else if (macOS)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ListenLocalhost(5002, l =>
        {
            l.Protocols = HttpProtocols.Http2;
        });
    });
}

builder.Services.AddSingleton<HealthServiceImpl>();
builder.Services.AddHostedService<HealthCheckBackgroundService>();

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IToppingData, ToppingData>();
builder.Services.AddSingleton<ICrustData, CrustData>();
builder.Services.AddSingleton<IngredientsImpl>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGrpcService<IngredientsImpl>();
app.MapGrpcService<HealthServiceImpl>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
