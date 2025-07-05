using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using FluentTestScaffold.Core;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.Autofac;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

public class Examples
{
    /// <summary>
    /// Demonstrates the supported Autofac and .Net Ioc Containers and how to register services 
    /// </summary>
    public void IOC_Examples()
    {
        // We need
        // 1. A DbContext
        // 2. A UserRequestContext
        // 3. An AuthService
        // 4. A ShoppingCart


        // Default .net IOC
        new TestScaffold()
            .UseIoc(serviceBuilder =>
            {
                serviceBuilder.Container.AddSingleton(_ => TestDbContextFactory.Create());
                serviceBuilder.Container.AddScoped<IUserRequestContext, UserRequestContext>();
                serviceBuilder.Container.AddTransient<IAuthService, AuthService>();
                serviceBuilder.Container.AddTransient<ShoppingCart>();
            });
        
        // Autofac IOC
        new TestScaffold()
            .UseAutofac(serviceBuilder =>
            {
                serviceBuilder.Container.Register<TestDbContext>(_ => TestDbContextFactory.Create()).SingleInstance();
                serviceBuilder.Container.RegisterType<UserRequestContext>().As<IUserRequestContext>()
                    .InstancePerLifetimeScope();
                serviceBuilder.Container.RegisterType<AuthService>().As<IAuthService>();
                serviceBuilder.Container.RegisterType<ShoppingCart>();
            });

        // Autofac IOC with Custom Service Builder
        var email = "fts@test.com";
        var password = "Supper Secret Pa$$word";

        var testScaffold = new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
            {
                // Custom App Service Builder to register common services. 
                serviceBuilder.RegisterAppServices(requestContext => requestContext.AuthenticateUser(email, password));

                // Register service under test
                serviceBuilder.Container.RegisterType<ShoppingCartService>();
            });
    }

    /// <summary>
    /// Demonstrates how to use Builders to add data to the test scaffold. Uses the supported EF Core builder
    /// </summary>
    public void Builders()
    {
        var user = new User(Guid.NewGuid(), "Bob", "bob@test.com", "Password", DateTime.Now.AddYears(-15));
        var testScaffold = new TestScaffold()
                .UseAutofac(new AutofacAppServicesBuilder(), serviceBuilder =>
                {
                    serviceBuilder.RegisterAppServices(requestContext => requestContext.AuthenticateUser(user.Email, user.Password));
                    serviceBuilder.Container.RegisterType<ShoppingCartService>();
                });

        
        // Adding data using the default EF Core Builder
        testScaffold
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .With(user) //Add any EF data type
            .WithRange(new List<Item>()
            {
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Minions, Price = 21},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Avengers, Price = 24},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.DeadPool, Price = 14, AgeRestriction = 15}
            })
            .Build();
            
        
        // Adding data using a custom builder
        testScaffold.UsingBuilder<InventoryBuilder>()
            .With(user)
            .WithDefaultCatalogue()
            .Build();
        
        // Chaining Builders
        testScaffold.UsingBuilder<UserBuilder>()
            //Setup standard users
            .WithOver18User(out var over18UserId)
            .WithUnder18User(out var under18UserId)
            //Setup User Shopping Carts
            .WithShoppingCart(under18UserId)
            .WithShoppingCart(over18UserId)
            .Build()
            .UsingBuilder<InventoryBuilder>()
            //Setup Inventory
            .WithRange(new List<Item>()
            {
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Minions, Price = 21},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Avengers, Price = 24},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.DeadPool, Price = 14, AgeRestriction = 15}
            }).Build();
        
    }

    /// <summary>
    /// Demonstrates how AutoDiscovery can be used to automatically register Builders and DataTemplates
    /// </summary>
    public void AutoDiscoveryAndDataTemplates()
    {
        var param1 = 32;
        var param2 = Guid.NewGuid();
        var param3 = "Hello World";
        // Config options can be passed to the Service Builder to control Auto Discovery
        new TestScaffold(new ConfigOptions
            {
                // Enabled by default
                AutoDiscovery = AutoDiscovery.Builders | AutoDiscovery.DataTemplates,
                Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
            })
            .UseIoc()
            .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateMultipleParameters), 
                param1, param2, param3);
        
        
        // Using custom service builder to enable Auto Discovery
        new TestScaffold()
            .UseAutofac(new AutofacAppServicesBuilder())
            .WithTemplate(nameof(ApplicationDataTemplates.DefaultCatalogueAndUsers));

    }
    
    /// <summary>
    /// Demonstrates how to use the TestContext to store and retrieve data for later use in your tests
    /// </summary>
    public void TestContext()
    {
        // Builders and DataTemplates can set test context for use in tests
       var testScaffold = new TestScaffold()
            .UseIoc()
            .WithTemplate(nameof(ApplicationDataTemplates.DefaultCatalogueAndUsers));
       
       // This data template stores the UserId in the TestContext
       // Grab the Over18UserId
       var userId = testScaffold.TestScaffoldContext.Get<Guid>("Over18UserId");
    }
    
    /// <summary>
    /// Demonstrates it all together in a Test
    /// </summary>
    [Test]
    public void IntegrationTest()
    {
        // Test Adding Age Restricted Content
        
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