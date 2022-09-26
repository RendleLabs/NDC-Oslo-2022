using Grpc.Core;
using Orders.Protos;

namespace Shop;

public class Worker : BackgroundService
{
    private readonly OrderService.OrderServiceClient _orderServiceClient;
    private readonly ILogger<Worker> _logger;

    public Worker(OrderService.OrderServiceClient orderServiceClient, ILogger<Worker> logger)
    {
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stream = _orderServiceClient.Subscribe(new SubscribeRequest());

                await foreach (var notification in stream.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation("Order: {CrustId} with {ToppingIds} due by {DueBy}",
                        notification.CrustId,
                        string.Join(", ", notification.ToppingIds),
                        notification.DueBy.ToDateTimeOffset().ToLocalTime().ToString("t"));
                }
            }
            catch (OperationCanceledException)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
