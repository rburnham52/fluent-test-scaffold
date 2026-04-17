using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using NUnit.Framework;


namespace FluentTestScaffold.Tests;

public class AsyncBddTests
{
    private TestScaffold CreateTestScaffold()
    {
        return new TestScaffold()
            .UsingNunit()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                serviceBuilder.RegisterAppServices();
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            })
            .WithTemplate<ApplicationDataTemplates>(dt => dt.DefaultCatalogueAndUsers());
    }

    [Test]
    public async Task CanRunAsyncBddTestWithInlineSteps()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("User can not add age restricted item to cart when under aged")
            .GivenAsync<IUserRequestContext>("A user is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
                        UserBuilder.Under18User.Password));
            })
            .WhenAsync("The user attempts to add an item to the shopping cart", async ts =>
            {
                var dbContext = ts.Resolve<TestDbContext>();
                var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
                var shoppingCartService = ts.Resolve<ShoppingCartService>();
                await ts.CatchAsync<InvalidOperationException>(async () =>
                    await Task.Run(() => shoppingCartService.AddItemToCart(item!.Id)));
            })
            .ThenAsync("The user should not be able to add the item to the shopping cart", async ts =>
            {
                await ts.HandleAsync<InvalidOperationException>(async ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.AreEqual("You must be over 15 to add this item", ex.Message);
                    await Task.CompletedTask;
                });
            });
    }

    [Test]
    public async Task CanRunAsyncBddTestWithMethodReferences()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("User can not add age restricted item to cart when under aged")
            .GivenAsync<IUserRequestContext>("A user is authenticated", AuthenticateUnderAgeUserAsync)
            .WhenAsync("The user attempts to add an item to the shopping cart", WhenTheUserAddsItemToCartAsync)
            .ThenAsync("The user should not be able to add the item to the shopping cart", ThenTheUserCanNotAddAgeRestrictedContentAsync);
    }

    [Test]
    public async Task CanChainAsyncAndSyncSteps()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("User can not add age restricted item to cart when under aged")
            .GivenAsync<IUserRequestContext>("A user is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
                        UserBuilder.Under18User.Password));
            })
            .WhenAsync("The user attempts to add an item to the shopping cart", (Action<TestScaffold>)(ts =>
            {
                var dbContext = ts.Resolve<TestDbContext>();
                var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
                var shoppingCartService = ts.Resolve<ShoppingCartService>();
                ts.Catch<InvalidOperationException>(() => shoppingCartService.AddItemToCart(item!.Id));
            }))
            .ThenAsync("The user should not be able to add the item to the shopping cart", (Action<TestScaffold>)(ts =>
            {
                ts.Handle<InvalidOperationException>(ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.AreEqual("You must be over 15 to add this item", ex.Message);
                });
            }));
    }

    [Test]
    public async Task CanUseAsyncAndStep()
    {
        var executionOrder = new System.Collections.Generic.List<string>();

        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("Async And step works correctly")
            .GivenAsync("A precondition", async ts =>
            {
                executionOrder.Add("given");
                await Task.CompletedTask;
            })
            .AndAsync("An additional precondition", async ts =>
            {
                executionOrder.Add("and");
                await Task.CompletedTask;
            })
            .WhenAsync("An action is performed", async ts =>
            {
                executionOrder.Add("when");
                await Task.CompletedTask;
            })
            .ThenAsync("The outcome is verified", async ts =>
            {
                executionOrder.Add("then");
                await Task.CompletedTask;
            });

        Assert.AreEqual(4, executionOrder.Count);
        Assert.AreEqual("given", executionOrder[0]);
        Assert.AreEqual("and", executionOrder[1]);
        Assert.AreEqual("when", executionOrder[2]);
        Assert.AreEqual("then", executionOrder[3]);
    }

    [Test]
    public async Task CanUseAsyncGivenWithInjectedService()
    {
        var testScaffold = CreateTestScaffold();
        var authenticated = false;

        await testScaffold
            .Scenario("Async service injection works in Given step")
            .GivenAsync<IUserRequestContext>("A user is authenticated via async step", async requestContext =>
            {
                await Task.Run(() =>
                {
                    requestContext.AuthenticateUser(UserBuilder.Over18User.Email,
                        UserBuilder.Over18User.Password);
                    authenticated = true;
                });
            })
            .ThenAsync("The user should be authenticated", async ts =>
            {
                Assert.IsTrue(authenticated);
                var userContext = ts.Resolve<IUserRequestContext>();
                Assert.IsNotNull(userContext.CurrentUser);
                Assert.AreEqual(UserBuilder.Over18User.Email, userContext.CurrentUser!.Email);
                await Task.CompletedTask;
            });
    }

    [Test]
    public async Task CanUseAsyncWhenWithInjectedService()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("Async service injection works in When step")
            .GivenAsync<IUserRequestContext>("A user over 18 is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Over18User.Email,
                        UserBuilder.Over18User.Password));
            })
            .WhenAsync<ShoppingCartService>("The user adds an item to the cart", async shoppingCartService =>
            {
                var dbContext = testScaffold.Resolve<TestDbContext>();
                var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.Minions);
                await Task.Run(() => shoppingCartService.AddItemToCart(item!.Id));
            })
            .ThenAsync("The item should be in the cart", async ts =>
            {
                var dbContext = ts.Resolve<TestDbContext>();
                var userContext = ts.Resolve<IUserRequestContext>();
                var cart = dbContext.ShoppingCart.FirstOrDefault(c => c.UserId == userContext.CurrentUser!.Id);
                Assert.IsNotNull(cart);
                Assert.IsTrue(cart!.Inventory.Any(i => i.Title == Defaults.CatalogueItems.Minions));
                await Task.CompletedTask;
            });
    }

    [Test]
    public async Task AsyncCatchHandlesExpectedExceptionType()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("Async Catch stores exception for later assertion")
            .GivenAsync<IUserRequestContext>("A user is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
                        UserBuilder.Under18User.Password));
            })
            .WhenAsync("The user attempts a restricted action", async ts =>
            {
                var dbContext = ts.Resolve<TestDbContext>();
                var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
                var shoppingCartService = ts.Resolve<ShoppingCartService>();
                await ts.CatchAsync<InvalidOperationException>(async () =>
                    await Task.Run(() => shoppingCartService.AddItemToCart(item!.Id)));
            })
            .ThenAsync("The exception should be available for assertion", async ts =>
            {
                await ts.HandleAsync<InvalidOperationException>(async ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.That(ex.Message, Does.Contain("over 15"));
                    await Task.CompletedTask;
                });
            });
    }

    [Test]
    public async Task AsyncCatchRethrowsUnexpectedExceptionType()
    {
        var testScaffold = CreateTestScaffold();

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await testScaffold.CatchAsync<ArgumentException>(async () =>
            {
                await Task.Run(() => throw new InvalidOperationException("unexpected"));
            });
        });
    }

    [Test]
    public async Task AsyncThenWithInjectedServiceWorks()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("Async Then with service injection")
            .GivenAsync<IUserRequestContext>("A user over 18 is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Over18User.Email,
                        UserBuilder.Over18User.Password));
            })
            .ThenAsync<IUserRequestContext>("The user context should have the user", async userContext =>
            {
                Assert.IsNotNull(userContext.CurrentUser);
                Assert.AreEqual(UserBuilder.Over18User.Email, userContext.CurrentUser!.Email);
                await Task.CompletedTask;
            });
    }

    [Test]
    public async Task AsyncAndWithInjectedServiceWorks()
    {
        var testScaffold = CreateTestScaffold();

        await testScaffold
            .Scenario("Async And with service injection")
            .GivenAsync<IUserRequestContext>("A user over 18 is authenticated", async requestContext =>
            {
                await Task.Run(() =>
                    requestContext.AuthenticateUser(UserBuilder.Over18User.Email,
                        UserBuilder.Over18User.Password));
            })
            .AndAsync<IUserRequestContext>("The user context is verified", async userContext =>
            {
                Assert.IsNotNull(userContext.CurrentUser);
                await Task.CompletedTask;
            })
            .ThenAsync("Done", async ts =>
            {
                await Task.CompletedTask;
            });
    }

    // === Async step method references ===

    private static async Task AuthenticateUnderAgeUserAsync(IUserRequestContext requestContext)
    {
        await Task.Run(() =>
            requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
                UserBuilder.Under18User.Password));
    }

    private static async Task WhenTheUserAddsItemToCartAsync(TestScaffold testScaffold)
    {
        var dbContext = testScaffold.Resolve<TestDbContext>();
        var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        await testScaffold.CatchAsync<InvalidOperationException>(async () =>
            await Task.Run(() => shoppingCartService.AddItemToCart(item!.Id)));
    }

    private static async Task ThenTheUserCanNotAddAgeRestrictedContentAsync(TestScaffold testScaffold)
    {
        await testScaffold.HandleAsync<InvalidOperationException>(async ex =>
        {
            Assert.IsNotNull(ex);
            Assert.AreEqual("You must be over 15 to add this item", ex.Message);
            await Task.CompletedTask;
        });
    }
}
