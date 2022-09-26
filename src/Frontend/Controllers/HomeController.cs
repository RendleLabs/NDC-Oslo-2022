using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using Ingredients.Protos;

namespace Frontend.Controllers;

public class HomeController : Controller
{
    private readonly IngredientsService.IngredientsServiceClient _ingredients;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IngredientsService.IngredientsServiceClient ingredients, 
        ILogger<HomeController> logger)
    {
        _ingredients = ingredients;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var toppings = await GetToppingsAsync();
        
        var crusts = await GetCrustsAsync();
        
        var viewModel = new HomeViewModel(toppings, crusts);
        
        return View(viewModel);
    }

    private async Task<List<ToppingViewModel>> GetToppingsAsync()
    {
        var response = await _ingredients.GetToppingsAsync(new GetToppingsRequest());

        return response.Toppings
            .Select(t => new ToppingViewModel(t.Id, t.Name, t.Price))
            .ToList();
    }

    private async Task<List<CrustViewModel>> GetCrustsAsync()
    {
        var response = await _ingredients.GetCrustsAsync(new GetCrustsRequest());

        return response.Crusts
            .Select(c => new CrustViewModel(c.Id, c.Name, c.Size, c.Price))
            .OrderBy(t => t.Size)
            .ThenBy(t => t.Id)
            .ToList();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}