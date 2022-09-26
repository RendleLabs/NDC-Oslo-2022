using Grpc.Core;
using Ingredients.Data;
using Ingredients.Protos;

namespace Ingredients.Services;

public class IngredientsImpl : IngredientsService.IngredientsServiceBase
{
    private readonly IToppingData _toppingData;

    public IngredientsImpl(IToppingData toppingData)
    {
        _toppingData = toppingData;
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
                        Price = t.Price
                    })
            }
        };

        return response;
    }
}