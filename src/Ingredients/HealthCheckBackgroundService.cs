using Grpc.Health.V1;
using Grpc.HealthCheck;
using Ingredients.Data;

namespace Ingredients;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly ICrustData _crustData;
    private readonly HealthServiceImpl _healthServiceImpl;

    public HealthCheckBackgroundService(ICrustData crustData, HealthServiceImpl healthServiceImpl)
    {
        _crustData = crustData;
        _healthServiceImpl = healthServiceImpl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (await _crustData.IsHealthyAsync())
            {
                _healthServiceImpl.SetStatus("ingredients", HealthCheckResponse.Types.ServingStatus.Serving);
            }
            else
            {
                _healthServiceImpl.SetStatus("ingredients", HealthCheckResponse.Types.ServingStatus.NotServing);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}