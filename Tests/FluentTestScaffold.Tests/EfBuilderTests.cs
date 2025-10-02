using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using FluentTestScaffold.Core;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using FluentTestScaffold.Tests.CustomBuilder.DefaultIoc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class EfBuilderTests
{
    [Test]
    public void EFBuilder_Can_Access_User_From_TestScaffolds_DbContext()
    {
        var dbContext = TestDbContextFactory.Create();
        var userId = Guid.Parse("65579043-8112-480C-A885-C6157947F0F3");

        new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .With(new User(
                id: userId,
                email: "Bob@test.com",
                password: "",
                name: "Bob",
                dateOfBirth: DateTime.Now.AddYears(-15)
            ))
            .Build();

        var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        Assert.IsNotNull(user);
        Assert.IsTrue(user?.Id == userId);
    }

    [Test]
    public void EBBuilder_Can_Extend_EfBuilder()
    {
        var dbContext = TestDbContextFactory.Create();
        var userId = Guid.Parse("36A6736A-F8AC-4FA2-B33E-0ACB14776C0F");
        new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .WithDefaultCatalogue()
            .Build()
            .UsingBuilder<UserBuilder>()
            .With(new User(
                id: userId,
                email: "Bob@test.com",
                password: "",
                name: "Bob",
                dateOfBirth: DateTime.Now.AddYears(-15)
            ))
            .WithShoppingCart(userId)
            .Build();

        var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        var shopping = dbContext.ShoppingCart.FirstOrDefault(s => s.User.Id == userId);
        var items = dbContext.Items.ToList();

        Assert.Multiple(() =>
        {
            //User Added to dbContext
            Assert.IsNotNull(user);
            Console.WriteLine($"Matched User:{user?.Id}, Expected User: {userId}");
            Assert.IsTrue(user?.Id == userId, "User not found");

            //Shopping Cart Added to DBContext
            Assert.IsNotNull(shopping);

            //Default items added to DBContext
            Assert.IsTrue(items.Count == 3, "Item count did not match");
        });
    }

    [Test]
    public void EBBuilder_Can_Defer_Adding_To_DbContext()
    {
        var dbContext = TestDbContextFactory.Create();
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .WithDefaultCatalogue()
            .UsingTestScaffold();

        var userId = Guid.NewGuid();
        testScaffold.UsingBuilder<InventoryBuilder>()
            .With(new User(userId, "Bob", "bob@test.com", "SuperSecret123",
                DateTime.Now.AddYears(-12)))
            .Build();

        var hasItem = dbContext.Items.Any(i => i.Title == Defaults.CatalogueItems.Avengers);
        var hasUser = dbContext.Users.Any(i => i.Id == userId);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(hasItem, "DbContext was not seeded with deferred builder");
            Assert.IsTrue(hasUser, "DbContext was not seeded with second call to deferred builder");
        });
    }

    [Test]
    public void EBBuilder_Can_Use_EFBuilder_In_DataTemplate()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(new DotnetIocAppServicesBuilder(),
                ctx =>
                {
                    ctx.Container.AddSingleton(_ => TestDbContextFactory.Create());
                    ctx.RegisterAppServices();
                })
            .WithTemplate<ApplicationDataTemplates>(dt => dt.DefaultCatalogueAndUsers());


        var dbContext = testScaffold.Resolve<TestDbContext>();

        var hasItem = dbContext.Items.Any(i => i.Title == Defaults.CatalogueItems.Avengers);
        var hasUser = dbContext.Users.Any(i => i.Name == "Jeff");
        Assert.Multiple(() =>
        {
            Assert.IsTrue(hasItem, "DbContext was not seeded from Template.");
            Assert.IsTrue(hasUser, "DbContext was not seeded from Template.");
        });
    }

    [Test]
    public void EBBuilder_Can_Conditionally_Build_Data()
    {
        var dbContext = TestDbContextFactory.Create();
        var userId = Guid.Parse("36A6736A-F8AC-4FA2-B33E-0ACB14776C0F");
        var itemToAdd = new Item()
        {
            Id = Guid.NewGuid(),
            Title = "Test Item",
            Price = 10
        };

        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .With(itemToAdd)
            .Build()
            .UsingBuilder<UserBuilder>()
            .With(new User(
                id: userId,
                email: "Bob@test.com",
                password: "",
                name: "Bob",
                dateOfBirth: DateTime.Now.AddYears(-15)
            ))
            .WithShoppingCart(userId)
            .Build(); //Build to ensure the Shopping cart is created

        testScaffold
            .UsingBuilder<UserBuilder>()
            .If(false, builder => builder.WithItem(userId, itemToAdd))
            .Build();

        var user = dbContext.Users.Include(user => user.ShoppingCart)
            .ThenInclude(shoppingCart => shoppingCart!.Inventory).FirstOrDefault(u => u.Id == userId);
        var hasItemInCart = user?.ShoppingCart?.Inventory?.Exists(i => i.Id == itemToAdd.Id) ?? false;
        //Ensure the item was not added to the users shopping cart
        Assert.IsFalse(hasItemInCart, "Item has not been added to the users shopping cart.");

        testScaffold
            .UsingBuilder<UserBuilder>()
            .If(true, builder => builder.WithItem(userId, itemToAdd))
            .Build();

        hasItemInCart = user?.ShoppingCart?.Inventory?.Exists(i => i.Id == itemToAdd.Id) ?? false;
        // Ensure the item was added to the users shopping cart

        Assert.IsTrue(hasItemInCart, "Item has not been added to the users shopping cart.");
    }

    [Test]
    public void EBBuilder_Switching_Builders_Triggers_Build()
    {
        var dbContext = TestDbContextFactory.Create();
        var itemToAdd = new Item()
        {
            Id = Guid.NewGuid(),
            Title = "Test Item",
            Price = 10
        };

        new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .With(itemToAdd)
            .UsingBuilder<UserBuilder>();

        var item = dbContext.Items.FirstOrDefault(i => i.Id == itemToAdd.Id);
        //Ensure the item was not added to the users shopping cart
        Assert.IsNotNull(item, "Build has not been called to trigger SaveChanges");
    }

    [Test]
    public void EBBuilder_Switching_To_TestScaffold_Triggers_Build()
    {
        var dbContext = TestDbContextFactory.Create();
        var itemToAdd = new Item()
        {
            Id = Guid.NewGuid(),
            Title = "Test Item",
            Price = 10
        };

        new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .With(itemToAdd)
            .UsingTestScaffold();

        var item = dbContext.Items.FirstOrDefault(i => i.Id == itemToAdd.Id);
        //Ensure the item was not added to the users shopping cart
        Assert.IsNotNull(item, "Build has not been called to trigger SaveChanges");
    }


    [Test]
    public void EBBuilder_Can_Merge_Into_Existing_Entity()
    {
        var dbContext = TestDbContextFactory.Create();

        var itemId = Guid.NewGuid();

        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<InventoryBuilder>()
            .With(new Item()
            {
                Id = itemId,
                Title = Defaults.CatalogueItems.Avengers,
                Price = 24
            })
            .Build();

        var updatedItem = new Item { Id = itemId, Price = 30 };
        testScaffold
            .UsingBuilder<InventoryBuilder>()
            .Merge(updatedItem)
            .Build();

        var item = dbContext.Items.FirstOrDefault(u => u.Id == itemId);

        item.Should().NotBeNull().And.BeEquivalentTo(updatedItem,
            options => options.Excluding(x => x.Title));
        item!.Title.Should().Be(Defaults.CatalogueItems.Avengers);
    }

    [Test]
    public void EfCoreBuilder_Build_Should_Apply_All_Enqueued_Actions_And_SaveChanges()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var user1 = new User(userId1, "John Doe", "john@test.com", "password123", DateTime.Now.AddYears(-25));
        var user2 = new User(userId2, "Jane Smith", "jane@test.com", "password456", DateTime.Now.AddYears(-30));

        // Act
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .With(user1)
            .With(user2)
            .Build();

        // Assert
        var savedUsers = dbContext.Users.ToList();
        savedUsers.Should().HaveCount(2);

        var savedUser1 = dbContext.Users.FirstOrDefault(u => u.Id == userId1);
        var savedUser2 = dbContext.Users.FirstOrDefault(u => u.Id == userId2);

        Assert.Multiple(() =>
        {

            // Verify that Build() returns a TestScaffold instance
            Assert.IsNotNull(testScaffold);
            Assert.IsInstanceOf<TestScaffold>(testScaffold);

            // Verify that all entities were saved to the database
            Assert.IsNotNull(savedUser1, "First user should be saved to database");
            Assert.IsNotNull(savedUser2, "Second user should be saved to database");

            // Verify entity properties are correctly saved
            Assert.AreEqual("John Doe", savedUser1?.Name);
            Assert.AreEqual("john@test.com", savedUser1?.Email);
            Assert.AreEqual("Jane Smith", savedUser2?.Name);
            Assert.AreEqual("jane@test.com", savedUser2?.Email);
        });
    }

    [Test]
    public void EfCoreBuilder_Build_Should_Handle_Empty_Queue_Gracefully()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();

        // Act
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .Build();

        // Assert
        Assert.Multiple(() =>
        {
            // Verify that Build() returns a TestScaffold instance even with no entities
            Assert.IsNotNull(testScaffold);
            Assert.IsInstanceOf<TestScaffold>(testScaffold);

            // Verify that no entities were added to the database
            Assert.AreEqual(0, dbContext.Users.Count());
            Assert.AreEqual(0, dbContext.Items.Count());
        });
    }

    [Test]
    public void EfCoreBuilder_Build_Should_Call_SaveChanges_After_Base_Build()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var user = new User(userId, "Test User", "test@test.com", "password", DateTime.Now.AddYears(-20));

        // Act - Add entity and build
        var testScaffold = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .With(user)
            .Build();

        // Assert - Verify that changes are persisted to the database
        var persistedUser = dbContext.Users.FirstOrDefault(u => u.Id == userId);
        Assert.Multiple(() =>
        {
            // Verify that there are no unsaved changes in the DbContext
            var hasChanges = dbContext.ChangeTracker.HasChanges();
            Assert.IsFalse(hasChanges, "DbContext should have no pending changes after Build()");

            Assert.IsNotNull(persistedUser, "User should be persisted to database after Build()");
            Assert.AreEqual(userId, persistedUser?.Id);
            Assert.AreEqual("Test User", persistedUser?.Name);

            // Verify that Build() returns a TestScaffold instance
            Assert.IsNotNull(testScaffold);
            Assert.IsInstanceOf<TestScaffold>(testScaffold);
        });
    }

    [Test]
    public void EfCoreBuilder_Build_Should_Not_Call_SaveChanges_After_Base_Build_With_SaveChanges_False()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var user = new User(userId, "Test User", "test@test.com", "password", DateTime.Now.AddYears(-20));

        var efCoreBuilder = new TestScaffold()
            .UseIoc(ctx =>
            {
                ctx.Container.AddSingleton(_ => dbContext);
            })
            .UsingBuilder<EfCoreBuilder<TestDbContext>>()
            .With(user);

        // Make a change to the entity to verify SaveChanges was not called
        user.Name = "Test User";
        dbContext.Users.Update(user);

        var hasChanges = dbContext.ChangeTracker.HasChanges();
        Assert.IsTrue(hasChanges, "DbContext should have pending changes after change to entity");

        // Act - Call Build with saveChanges false
        efCoreBuilder.Build(saveChanges:false);

        // Assert - Verify that changes are persisted to the database
        hasChanges = dbContext.ChangeTracker.HasChanges();

        // Verify that there are no unsaved changes in the DbContext
        Assert.IsFalse(hasChanges, "DbContext should have no pending changes after Build()");
    }
}

