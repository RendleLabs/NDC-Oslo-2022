using AuthHelp;
using Grpc.Core;
using Ingredients.Protos;
using Orders.Protos;

var builder = WebApplication.CreateBuilder(args);

var macOS = OperatingSystem.IsMacOS();

// Add services to the container.
builder.Services.AddControllersWithViews();

// macOS doesn't like HTTPS
var binding = macOS ? "http" : "https";

var defaultIngredientsUri = macOS ? "http://localhost:5002" : "https://localhost:5003";
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

var defaultOrdersUri = macOS ? "http://localhost:5004" : "https://localhost:5005";
var ordersUri = builder.Configuration.GetServiceUri("orders", binding)
                ?? new Uri(defaultOrdersUri);

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o => { o.Address = ordersUri; })
    .ConfigureChannel((channel) =>
    {
        var callCredentials = CallCredentials.FromInterceptor(async (_, metadata) =>
        {
            var token = JwtHelper.GenerateJwtToken("frontend");
            metadata.Add("Authorization", $"Bearer {token}");
        });
        channel.Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, callCredentials);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();