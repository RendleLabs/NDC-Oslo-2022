using Orders.Protos;
using Shop;

var macOS = OperatingSystem.IsMacOS();
var binding = macOS ? "http" : "https";

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var defaultOrdersUri = macOS ? "http://localhost:5004" : "https://localhost:5005";

        services.AddGrpcClient<OrderService.OrderServiceClient>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var ordersUri = configuration.GetServiceUri("orders", binding)
                            ?? new Uri(defaultOrdersUri);
            options.Address = ordersUri;
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();