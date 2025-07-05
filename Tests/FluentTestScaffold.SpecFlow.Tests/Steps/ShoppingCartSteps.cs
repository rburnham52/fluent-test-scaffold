using FluentTestScaffold.Core;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests;
using FluentTestScaffold.Tests.CustomBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TechTalk.SpecFlow.Assist;

namespace FluentTestScaffold.Specflow.Tests.Steps;

[Binding]
public class ShoppingCartSteps
{
    private ScenarioContext _scenarioContext;

    public ShoppingCartSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void SetupTestUsers()
    {
        var dbContext = TestDbContextFactory.Create();
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
                ctx.Container.AddTransient<IAuthService, AuthService>();
                ctx.Container.AddScoped<IUserRequestContext>(c =>
                {
                    var requestContext = new UserRequestContext(c.GetRequiredService<IAuthService>());
                    var dbContext = c.GetRequiredService<TestDbContext>();
                    var user = dbContext.Users.FirstOrDefault(u => u.Id == Defaults.CurrentUserId);
                    requestContext.AuthenticateUser(user?.Email, user?.Password);
                    return requestContext;
                });
                ctx.Container.AddTransient<ShoppingCartService>();
            })
            .UsingBuilder<InventoryBuilder>()
            .WithDefaultCatalogue()
            .Build();

        _scenarioContext.Add(nameof(TestScaffold), testScaffold);
    }

    [Given(@"the Items")]
    public void GivenTheItems(Table table)
    {
        var items = table.CreateSet<Item>();
        var testScaffold = _scenarioContext.Get<TestScaffold>(nameof(TestScaffold));
        testScaffold.UsingBuilder<InventoryBuilder>()
            .WithRange(items)
            .Build();
    }

    [When(@"I Add (.*) to my Shopping Cart")]
    public void WhenIAddTheItems(string title)
    {
        var testScaffold = _scenarioContext.Get<TestScaffold>(nameof(TestScaffold));
        var dbContext = testScaffold.Resolve<TestDbContext>();

        var item = dbContext.Items.FirstOrDefault(i => i.Title == title);

        _scenarioContext.Add("ItemAdded", item);
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        try
        {
            shoppingCartService.AddItemToCart(item!.Id);
        }
        catch (Exception e)
        {
            _scenarioContext.Add("AddToCartError", e);
        }
    }

    [Then(@"I should see the item (.*) in my Shopping Cart")]
    public void ThenIShouldSeeTheseItemsInMyShoppingCart(string title)
    {
       var testScaffold = _scenarioContext.Get<TestScaffold>(nameof(TestScaffold)); 
        var dbContext = testScaffold.Resolve<TestDbContext>();

        var userId = _scenarioContext.Get<Guid?>("CurrentUserId");
        var item = dbContext.Items.FirstOrDefault(i => i.Title == title);

        var cart = dbContext.ShoppingCart.Include(s => s.Inventory).FirstOrDefault(u => u.UserId == userId);
        Assert.IsTrue(cart?.Inventory.Any(i => i.Id == item!.Id));
    }

    [Then(@"I should get the error ""(.*)"" from my Shopping Cart")]
    public void ThenIShouldGetTheErrorFromMyShoppingCart(string error)
    {
        var cartError = _scenarioContext.Get<InvalidOperationException>("AddToCartError");
        Assert.AreEqual(error, cartError.Message);
    }
}