using Ingredients.Protos;
using JaegerTracing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Orders.PubSub;
using Orders.Services;

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

// macOS doesn't like HTTPS
var defaultIngredientsUri = macOS ? "http://localhost:5002" : "https://localhost:5003";
var binding = macOS || runningInContainer ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding)
                     ?? new Uri(defaultIngredientsUri);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddOrderPubSub();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
