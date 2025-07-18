using System;
using System.Globalization;
using System.Linq;
using Autofac;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class IntegrationTest
{
    /// <summary>
    /// Example of a more complicated Component Integration Test.
    /// ShoppingCartService depends on UserContext which depends on AuthService to resolve the user. 
    /// -- Setup DB Structure with Custom EFBuilder
    /// -- Setup Dependant AuthServices
    /// -- Setup Ioc & Dependant Services
    /// -- Attempt Purchase of age restricted content
    /// </summary>
    [Test]
    public void ComponentIntegrationTest_UserCanAddToCart()
    {
        var email = "Jim@test.com";
        var password = "SupperSecretPa$$word";

        // Arrange
        var userId = Guid.Parse("A5A743C3-A02F-4CA3-94F8-B0ECAF4A6345");
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services. 
                serviceBuilder.RegisterAppServices(requestContext => requestContext.AuthenticateUser(email, password));

                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
                serviceBuilder.WithMock<ITimeService>(mock =>
                {
                    mock.Setup(c => c.GetTime()).Returns(TimeOnly.Parse("12:51:01", CultureInfo.CurrentCulture));
                    return mock;
                });
            })
            .UsingBuilder<InventoryBuilder>()
            .WithDefaultCatalogue()
            .Build()
            .UsingBuilder<UserBuilder>()
            .With(new User(
                id: userId,
                email: email,
                password: password,
                name: "Jimmy",
                dateOfBirth: DateTime.Now.AddYears(-15)
            ))
            .WithShoppingCart(userId)
            .Build();

        //Resolved the dbContext registered by the AutofacAppServicesBuilder
        var dbContext = testScaffold.Resolve<TestDbContext>();
        var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);

        // Act
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        shoppingCartService.AddItemToCart(item!.Id);

        // Assert
        var cart = dbContext.ShoppingCart.Include(s => s.Inventory).FirstOrDefault(u => u.UserId == userId);
        Assert.IsTrue(cart?.Inventory.Any(i => i.Id == item.Id));
    }


    [Test]
    public void ComponentIntegrationTest_UsingDataTemplates_UnderAged()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services. 
                serviceBuilder.RegisterAppServices();
                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            })
            .WithTemplate(nameof(ApplicationDataTemplates.DefaultCatalogueAndUsers));

        // Authenticate user initialised with the DataTemplate
        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(UserBuilder.Under18User.Email, UserBuilder.Under18User.Password);

        //Resolved the dbContext registered by the AutofacAppServicesBuilder
        var dbContext = testScaffold.Resolve<TestDbContext>();
        var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);

        // Attempt to add age restricted content with under age user
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        Assert.Throws<InvalidOperationException>(() => shoppingCartService.AddItemToCart(item!.Id));
    }

    [Test]
    public void ComponentIntegrationTest_UsingDataTemplates_OverAged()
    {
        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services. 
                serviceBuilder.RegisterAppServices();
                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            })
            .WithTemplate(nameof(ApplicationDataTemplates.DefaultCatalogueAndUsers));

        // Authenticate user initialised with the DataTemplate
        var requestContext = testScaffold.Resolve<IUserRequestContext>();
        requestContext.AuthenticateUser(UserBuilder.Over18User.Email, UserBuilder.Over18User.Password);

        //Resolved the dbContext registered by the AutofacAppServicesBuilder
        var dbContext = testScaffold.Resolve<TestDbContext>();
        var item = dbContext.Items.FirstOrDefault(i => i.Title == Defaults.CatalogueItems.DeadPool);

        // Attempt to add age restricted content with under age user
        var shoppingCartService = testScaffold.Resolve<ShoppingCartService>();
        shoppingCartService.AddItemToCart(item!.Id);

        // Get the UserId stored by the DataTemplate
        var userId = testScaffold.TestScaffoldContext.Get<Guid>("Over18UserId");

        var cart = dbContext.ShoppingCart.Include(s => s.Inventory).FirstOrDefault(u => u.UserId == userId);
        Assert.IsTrue(cart?.Inventory.Any(i => i.Id == item.Id));
    }

}