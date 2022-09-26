using Frontend.Models;
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
        var response = await _orders.PlaceOrderAsync(placeOrderRequest);

        ViewData["DueBy"] = response.DueBy.ToDateTimeOffset();
        return View();
    }
}