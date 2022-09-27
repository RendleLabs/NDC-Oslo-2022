using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;

namespace Ingredients.Services;

public class IngredientsImpl : IngredientsService.IngredientsServiceBase
{
    private readonly IToppingData _toppingData;
    private readonly ICrustData _crustData;

    public IngredientsImpl(IToppingData toppingData, ICrustData crustData)
    {
        _toppingData = toppingData;
        _crustData = crustData;
    }

    public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
    {
        var toppings = await _toppingData.GetAsync(context.CancellationToken);

        var response = new GetToppingsResponse
        {
            Toppings =
            {
                toppings.OrderBy(t => t.Id)
                    .Select(t => new Topping
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Price = t.Price,
                    })
            }
        };
        
        return response;
    }

    private static readonly DecrementToppingsResponse DecrementToppingsResponse = new();

    public override async Task<DecrementToppingsResponse> DecrementToppings(DecrementToppingsRequest request, ServerCallContext context)
    {
        var tasks = request.ToppingIds
            .Select(id => _toppingData.DecrementStockAsync(id, context.CancellationToken));
        await Task.WhenAll(tasks);
        return DecrementToppingsResponse;
    }

    private static readonly DecrementCrustsResponse DecrementCrustsResponse = new();
    
    public override async Task<DecrementCrustsResponse> DecrementCrusts(DecrementCrustsRequest request, ServerCallContext context)
    {
        var tasks = request.CrustIds
            .Select(id => _crustData.DecrementStockAsync(id, context.CancellationToken));
        await Task.WhenAll(tasks);
        return DecrementCrustsResponse;
    }

    public override async Task<GetCrustsResponse> GetCrusts(GetCrustsRequest request, ServerCallContext context)
    {
        var crusts = await _crustData.GetAsync(context.CancellationToken);

        var response = new GetCrustsResponse
        {
            Crusts =
            {
                crusts
                    .OrderBy(c => c.Id)
                    .Select(c => new Crust
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Size = c.Size,
                        Price = c.Price
                    })
            }
        };

        return response;
    }
}