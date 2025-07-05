using System.Net.Http.Json;
using FluentAssertions;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.WebApp.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace FluentTestScaffold.Sample.WebApp.Tests.Steps.ShoppingCart;

[Binding]
public class ShoppingCartSteps : TestScaffoldStep
{
    public ShoppingCartSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
    }

    [Given(@"the shop has the following items to purchase")]
    public void GivenTheShopHasTheFollowingItemsToPurchase(Table table)
    {
        var items = table.CreateSet<Item>().ToList();

        foreach (var item in items)
        {
            item.Id = Guid.NewGuid();
        }
        
        TestScaffold.WithData<TestDbContext, Item>(items.ToArray());
        
        TestScaffold.TestScaffoldContext.Set(items);
    }

    [Given(@"the user has selected the item ""(.*)""")]
    public void GivenTheUserHasSelectedTheItem(string itemTitle)
    {
        var items = TestScaffold.TestScaffoldContext.Get<List<Item>>();

        var item = items.Single(i => i.Title == itemTitle);
        
        TestScaffold.TestScaffoldContext.Set(item);
    }

    [When(@"the item is added to the shopping cart")]
    public async Task WhenTheItemIsAddedToTheShoppingCart()
    {
        var item = TestScaffold.TestScaffoldContext.Get<Item>();

        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();

        var response = await httpClient.PostAsJsonAsync(
            "/ShoppingCart",
            new AddItemToShoppingCartRequest(item.Id));
        
        TestScaffold.TestScaffoldContext.Set(response);
    }

    [Then(@"the item is added to the user's shopping cart")]
    public async Task ThenTheItemIsInTheUsersShoppingCart()
    {
        var user = TestScaffold.TestScaffoldContext.Get<User>();
        var item = TestScaffold.TestScaffoldContext.Get<Item>();

        using var scope = TestScaffold.ServiceProvider!.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var shoppingCart =
            await dbContext.ShoppingCart
                .Include(sc => sc.Inventory)
                .SingleAsync(sc => sc.UserId == user.Id);

        shoppingCart.Inventory
            .Should()
            .Contain(i => i.Id == item.Id);
    }

    [Then(@"the item is not in the user's shopping cart")]
    public async Task ThenTheItemIsNotInTheUsersShoppingCart()
    {
        var user = TestScaffold.TestScaffoldContext.Get<User>();
        var itemExpectedNotToHaveBeenAdded = TestScaffold.TestScaffoldContext.Get<Item>();

        using var scope = TestScaffold.ServiceProvider!.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var shoppingCart =
            await dbContext.ShoppingCart
                .Include(sc => sc.Inventory)
                .SingleOrDefaultAsync(sc => sc.UserId == user.Id);
        
        // If there is never any item added to the shopping cart, then the shopping cart will never exist so valid
        // assertion for a null shopping cart
        // If there are already other items added to the shopping cart, then the assertion passes if none of the items
        // already in the shopping cart is the item expected not to have been added.
        shoppingCart?.Inventory?
            .Should()
            .Match(inventory =>
                inventory == null || inventory.All(i => i.Id != itemExpectedNotToHaveBeenAdded.Id));
    }
}