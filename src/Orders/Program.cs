using Ingredients.Protos;
using Orders.Services;

var macOS = OperatingSystem.IsMacOS();

var builder = WebApplication.CreateBuilder(args);

// macOS doesn't like HTTPS
var defaultIngredientsUri = macOS ? "http://localhost:5002" : "https://localhost:5003";
var binding = macOS ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding)
                     ?? new Uri(defaultIngredientsUri);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
