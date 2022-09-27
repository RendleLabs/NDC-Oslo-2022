using Ingredients.Protos;
using Orders.Protos;

var builder = WebApplication.CreateBuilder(args);

var runningInContainer = "true".Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));
var macOS = OperatingSystem.IsMacOS();

// Add services to the container.
builder.Services.AddControllersWithViews();

// macOS doesn't like HTTPS
var binding = macOS || runningInContainer ? "http" : "https";

var defaultIngredientsUri = macOS ? "http://localhost:5002" : "https://localhost:5003";
var ingredientsUri = builder.Configuration.GetServiceUri("ingredients", binding)
                     ?? new Uri(defaultIngredientsUri);

builder.Services.AddGrpcClient<IngredientsService.IngredientsServiceClient>(o =>
{
    o.Address = ingredientsUri;
});

var defaultOrdersUri = macOS ? "http://localhost:5004" : "https://localhost:5005";
var ordersUri = builder.Configuration.GetServiceUri("orders", binding)
                     ?? new Uri(defaultOrdersUri);

builder.Services.AddGrpcClient<OrderService.OrderServiceClient>(o =>
{
    o.Address = ordersUri;
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
