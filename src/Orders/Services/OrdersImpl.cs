using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Orders.Protos;

namespace Orders.Services;

public class OrdersImpl : OrderService.OrderServiceBase
{
    private readonly IngredientsService.IngredientsServiceClient _ingredients;

    public OrdersImpl(IngredientsService.IngredientsServiceClient ingredients)
    {
        _ingredients = ingredients;
    }

    public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
    {
        var decrementToppingsRequest = new DecrementToppingsRequest
        {
            ToppingIds = { request.ToppingIds }
        };
        await _ingredients.DecrementToppingsAsync(decrementToppingsRequest,
            cancellationToken: context.CancellationToken);

        var decrementCrustsRequest = new DecrementCrustsRequest
        {
            CrustIds = { request.CrustId }
        };
        await _ingredients.DecrementCrustsAsync(decrementCrustsRequest,
            cancellationToken: context.CancellationToken);

        return new PlaceOrderResponse
        {
            DueBy = DateTimeOffset.UtcNow.AddMinutes(45).ToTimestamp(),
        };
    }
}