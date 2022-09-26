using Ingredients.Protos;

namespace Ingredients.Tests;

public class CrustsTests : IClassFixture<IngredientsApplicationFactory>
{
    private readonly IngredientsApplicationFactory _factory;

    public CrustsTests(IngredientsApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetsCrusts()
    {
        var client = _factory.CreateGrpcClient();
        var response = await client.GetCrustsAsync(new GetCrustsRequest());
        
        Assert.Collection(response.Crusts,
            t => Assert.Equal("deep9", t.Id),
            t => Assert.Equal("thin9", t.Id));
    }
}