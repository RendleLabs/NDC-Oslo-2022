using Frontend.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Orders.Protos;

namespace Frontend.Controllers;

[Route("orders")]
public class OrdersController : Controller
{
    private readonly OrderService.OrderServiceClient _orders;
    private readonly ILogger<OrdersController> _log;

    public OrdersController(OrderService.OrderServiceClient orders, ILogger<OrdersController> log)
    {
        _orders = orders;
        _log = log;
    }

    [HttpPost]
    public async Task<IActionResult> Order([FromForm]HomeViewModel viewModel)
    {
        var placeOrderRequest = new PlaceOrderRequest
        {
            ToppingIds =
            {
                viewModel.Toppings
                    .Where(t => t.Selected)
                    .Select(t => t.Id)
            },
            CrustId = viewModel.SelectedCrust,
        };
        var metadata = new Metadata();
        if (User?.Identity?.IsAuthenticated == true)
        {
            var authHeader = Request.Headers.Authorization[0];
            if (authHeader.StartsWith("Bearer "))
            {
                metadata.Add("Authorization", authHeader);
            }
        }
        try
        {
            var response = await _orders.PlaceOrderAsync(placeOrderRequest, metadata);
            ViewData["DueBy"] = response.DueBy.ToDateTimeOffset();
            return View();
        }
        catch (RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}