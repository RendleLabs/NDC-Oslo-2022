using System.Security.Claims;
using AuthHelp;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orders.PubSub;
using Orders.Services;

var macOS = OperatingSystem.IsMacOS();

var builder = WebApplication.CreateBuilder(args);

// macOS doesn't like HTTPS
var defaultIngredientsUri = macOS ? "http://localhost:5002" : "https://localhost:5003";
var binding = macOS ? "http" : "https";

var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding)
                     ?? new Uri(defaultIngredientsUri);

builder.Services.AddHttpClient("ingredients")
    .ConfigurePrimaryHttpMessageHandler(DevelopmentModeCertificateHelper.CreateClientHandler);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o => { o.Address = ingredientsUri; })
    .ConfigureChannel((provider, options) =>
    {
        options.HttpHandler = null;
        options.HttpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("ingredients");
        options.DisposeHttpClient = true;
    });

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddOrderPubSub();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor = false,
            ValidateLifetime = false,
            IssuerSigningKey = JwtHelper.SecurityKey,
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(JwtBearerDefaults.AuthenticationScheme, p =>
    {
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        p.RequireClaim(ClaimTypes.Name);
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGrpcService<OrdersImpl>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
