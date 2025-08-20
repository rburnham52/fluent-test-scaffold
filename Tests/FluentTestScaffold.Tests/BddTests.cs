using System;
using System.Linq;
using Autofac;
using FluentTestScaffold.Core;
using FluentTestScaffold.Nunit;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;

using NUnit.Framework;

namespace FluentTestScaffold.Tests;

public class BddTests
{
    [Test]
    public void CanRunATestUsingBDDMethods()
    {
        var testScaffold = new TestScaffold()
            // Will be use to log scenario steps
            .UsingNunit()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services.
                serviceBuilder.RegisterAppServices();
                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            })
            .WithTemplate<ApplicationDataTemplates>(dt => dt.DefaultCatalogueAndUsers());

        testScaffold
            .Scenario("User can not add age restricted item to cart when under aged")
            .Given<IUserRequestContext>("A user is authenticated", requestContext =>
            {
                requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
                    UserBuilder.Under18User.Password);
            })
            .When("The user attempts to add an item to the shopping cart", ts =>
            {
                var dbContext = ts.Resolve<TestDbContext>();
                var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
                var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
                //Handles an expected exception and stores it in the TestContext for later Assertion
                ts.Catch<InvalidOperationException>(() => shoppingCartService.AddItemToCart(item!.Id));
            })
            .Then("The user should not be able to add the item to the shopping cart", ts =>
            {
                // Assert
                ts.Handle<InvalidOperationException>(ex =>
                {
                    Assert.IsNotNull(ex);
                    Assert.AreEqual($"You must be over 15 to add this item", ex.Message);
                });
            });
    }

    [Test]
    public void CanRunATestUsingFeatureClass()
    {
        var testScaffold = new TestScaffold()
            // Will be use to log scenario steps
            .UsingNunit()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services.
                serviceBuilder.RegisterAppServices();
                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            })
            .WithTemplate<ApplicationDataTemplates>(dt => dt.DefaultCatalogueAndUsers());

        testScaffold
            .Scenario("User can not add age restricted item to cart when under aged")
            .Given<IUserRequestContext>("A user is authenticated", AUserIsAuthenticated)
            .When("The user attempts to add an item to the shopping cart", WhenTheUserAddsItemToCart)
            .Then("The user should not be able to add the item to the shopping cart", ThenTheUserCanNotAddAgeRestrictedContent);
    }

    private static void ThenTheUserCanNotAddAgeRestrictedContent(TestScaffold testScaffold)
    {
        // Assert
        testScaffold.Handle<InvalidOperationException>(ex =>
        {
            Assert.IsNotNull(ex);
            Assert.AreEqual($"You must be over 15 to add this item", ex.Message);
        });
    }

    private static void WhenTheUserAddsItemToCart(TestScaffold testScaffold)
    {
        var dbContext = testScaffold.Resolve<TestDbContext>();
        var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        testScaffold.Catch<InvalidOperationException>(() => shoppingCartService.AddItemToCart(item!.Id));
    }

    private void AUserIsAuthenticated(IUserRequestContext requestContext)
    {
        requestContext.AuthenticateUser(UserBuilder.Under18User.Email,
            UserBuilder.Under18User.Password);
    }
}
