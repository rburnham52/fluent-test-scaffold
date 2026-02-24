# Getting Started

This guide walks you through the main features of Fluent Test Scaffold step by step. By the end, you'll know how to set up a test scaffold, register services, seed data with builders, reuse setup logic with data templates, and share state via the test context.

## Prerequisites

- .NET 6.0 or later
- A test project (NUnit, xUnit, MSTest, etc.)
- Entity Framework Core (if using database builders)

## Step 1 — Install the Packages

Start by adding the core package to your test project:

```bash
dotnet add package FluentTestScaffold.Core
```

Then add any extension packages you need:

```bash
# Entity Framework Core builders
dotnet add package FluentTestScaffold.EntityFrameworkCore

# Autofac IOC container (optional — .NET DI is built-in)
dotnet add package FluentTestScaffold.Autofac

# BDD-style API (optional)
dotnet add package FluentTestScaffold.Bdd
```

## Step 2 — Create Your First Test Scaffold

The `TestScaffold` class is the entry point. At its simplest, you create one, configure the IOC container, and you're ready to go.

```csharp
using FluentTestScaffold.Core;

[Test]
public void MyFirstScaffoldTest()
{
    var testScaffold = new TestScaffold()
        .UseIoc(ctx =>
        {
            ctx.Container.AddTransient<IGreetingService, GreetingService>();
        });

    // Resolve a service from the container
    var service = testScaffold.Resolve<IGreetingService>();

    // Act & Assert
    Assert.AreEqual("Hello, World!", service.Greet("World"));
}
```

**What's happening:**
1. `new TestScaffold()` — creates the scaffold instance.
2. `.UseIoc(...)` — registers services with the built-in .NET DI container and builds it.
3. `.Resolve<T>()` — retrieves a service from the container, just like your production code would.

> **Tip:** If your application uses Autofac, swap `UseIoc` for `UseAutofac` — see [IOC docs](ioc) for details.

## Step 3 — Seed Data with a Builder

Builders let you set up database state using a fluent API. The built-in `EfCoreBuilder` works with any EF Core `DbContext`.

```csharp
[Test]
public void SeedDataWithBuilder()
{
    using var dbContext = TestDbContextFactory.Create();
    var userId = Guid.NewGuid();

    new TestScaffold()
        .UseIoc(ctx =>
        {
            ctx.Container.AddSingleton(_ => dbContext);
        })
        .UsingBuilder<EfCoreBuilder<TestDbContext>>()
        .With(new User(
            id: userId,
            email: "alice@test.com",
            password: "Secret123",
            name: "Alice",
            dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-25))
        ))
        .Build();

    // The user is now persisted in the in-memory database
    var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
    Assert.IsNotNull(user);
    Assert.AreEqual("Alice", user.Name);
}
```

**What's happening:**
1. `.UsingBuilder<EfCoreBuilder<TestDbContext>>()` — switches the fluent API to the EF Core builder.
2. `.With(entity)` — enqueues an entity to be added (uses deferred execution).
3. `.Build()` — executes all enqueued actions and calls `SaveChanges()`.

You can also use `.WithRange(list)` to add multiple entities at once.

## Step 4 — Create a Custom Builder

Custom builders let you encapsulate common data setup behind descriptive methods. Create one by inheriting `EfCoreBuilder<TDbContext, TBuilder>`:

```csharp
public class ProductBuilder : EfCoreBuilder<TestDbContext, ProductBuilder>
{
    public ProductBuilder(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public ProductBuilder WithDefaultProducts()
    {
        WithRange(new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Widget", Price = 9.99m },
            new() { Id = Guid.NewGuid(), Name = "Gadget", Price = 24.99m },
        });
        return this;
    }

    public ProductBuilder WithShoppingCart(Guid userId)
    {
        With(new ShoppingCart { Id = Guid.NewGuid(), UserId = userId });
        return this;
    }
}
```

Then use it in a test:

```csharp
new TestScaffold()
    .UseIoc(ctx => ctx.Container.AddSingleton(_ => dbContext))
    .UsingBuilder<ProductBuilder>()
    .WithDefaultProducts()
    .WithShoppingCart(userId)
    .Build();
```

> **Convention:** Start builder methods with the `With` prefix to keep the fluent API readable.

## Step 5 — Chain Multiple Builders

You can switch between builders in a single chain. When you switch, the previous builder is automatically built.

```csharp
new TestScaffold()
    .UseIoc(ctx => ctx.Container.AddSingleton(_ => dbContext))
    // First builder — users
    .UsingBuilder<UserBuilder>()
    .With(new User( id: userId, email: "bob@test.com", password: "", name: "Bob",
        dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-30))))
    // Switching builder auto-builds the previous one
    .UsingBuilder<ProductBuilder>()
    .WithDefaultProducts()
    .WithShoppingCart(userId)
    .Build();
```

You can also switch back to the `TestScaffold` context at any time with `.UsingTestScaffold()`.

## Step 6 — Reuse Setup with Data Templates

When multiple tests share the same data setup, extract it into a **Data Template**. Mark the class with `[DataTemplates]` and inject dependencies via the constructor:

```csharp
[DataTemplates]
public class AppDataTemplates
{
    private readonly TestScaffold _testScaffold;

    public AppDataTemplates(TestScaffold testScaffold)
    {
        _testScaffold = testScaffold;
    }

    public void StandardUserWithProducts()
    {
        _testScaffold
            .UsingBuilder<UserBuilder>()
            .WithOver18User(out var userId)
            .Build()
            .UsingBuilder<ProductBuilder>()
            .WithDefaultProducts()
            .WithShoppingCart(userId)
            .Build();

        // Store the user ID so tests can retrieve it later
        _testScaffold.TestScaffoldContext.Set(userId, "StandardUserId");
    }
}
```

Apply it in a test with `WithTemplate`:

```csharp
var testScaffold = new TestScaffold()
    .UseIoc(ctx =>
    {
        ctx.Container.AddSingleton(_ => dbContext);
        ctx.Container.AddTransient<IAuthService, AuthService>();
    })
    .WithTemplate<AppDataTemplates>(dt => dt.StandardUserWithProducts());
```

Templates are automatically discovered and registered when using the default `ConfigOptions`. See [Data Templates](data-templates) and [Config Options](config-options) for more.

## Step 7 — Share State with TestScaffoldContext

The `TestScaffoldContext` is a shared dictionary accessible from builders, data templates, and your tests. It's useful for passing IDs or other values created during setup.

**Set a value** (from a builder or data template):

```csharp
// In a builder
SetTestContext("UserId", userId);

// Or directly
TestScaffoldContext.Set(userId, "UserId");
```

**Get a value** (in your test):

```csharp
var userId = testScaffold.TestScaffoldContext.Get<Guid>("UserId");
```

This avoids hard-coding IDs and keeps your setup and assertions loosely coupled.

## Step 8 — Put It All Together

Here's a complete integration test that combines IOC setup, data templates, service resolution, and assertions:

```csharp
[Test]
public void User_CanAddItemToCart()
{
    using var dbContext = TestDbContextFactory.Create();

    var testScaffold = new TestScaffold()
        .UseIoc(ctx =>
        {
            ctx.Container.AddSingleton(_ => dbContext);
            ctx.Container.AddTransient<IAuthService, AuthService>();
            ctx.Container.AddTransient<IUserRequestContext, UserRequestContext>();
            ctx.Container.AddTransient<ShoppingCartService>();
        })
        .WithTemplate<AppDataTemplates>(dt => dt.StandardUserWithProducts());

    // Retrieve the user ID stored by the data template
    var userId = testScaffold.TestScaffoldContext.Get<Guid>("StandardUserId");

    // Authenticate
    var requestContext = testScaffold.Resolve<IUserRequestContext>();
    requestContext.AuthenticateUser("test@example.com", "password");

    // Act
    var cartService = testScaffold.Resolve<ShoppingCartService>();
    var item = dbContext.Products.First();
    cartService.AddItemToCart(item.Id);

    // Assert
    var cart = dbContext.ShoppingCarts
        .Include(c => c.Items)
        .FirstOrDefault(c => c.UserId == userId);

    Assert.IsNotNull(cart);
    Assert.IsTrue(cart.Items.Any(i => i.Id == item.Id));
}
```

## Optional — BDD-Style Tests

If you prefer a Given/When/Then structure, install `FluentTestScaffold.Bdd` and write tests like this:

```csharp
var testScaffold = new TestScaffold()
    .UseIoc(ctx => { /* register services */ })
    .WithTemplate<AppDataTemplates>(dt => dt.StandardUserWithProducts());

testScaffold
    .Scenario("User can add an item to their cart")
    .Given<IUserRequestContext>("A user is authenticated", ctx =>
    {
        ctx.AuthenticateUser("test@example.com", "password");
    })
    .When("The user adds an item to the cart", ts =>
    {
        var item = ts.Resolve<TestDbContext>().Products.First();
        ts.Resolve<ShoppingCartService>().AddItemToCart(item.Id);
    })
    .Then("The item appears in the cart", ts =>
    {
        var userId = ts.TestScaffoldContext.Get<Guid>("StandardUserId");
        var cart = ts.Resolve<TestDbContext>().ShoppingCarts
            .Include(c => c.Items)
            .First(c => c.UserId == userId);
        Assert.IsTrue(cart.Items.Any());
    });
```

See the [BDD docs](bdd) for exception handling and logging support.

## Next Steps

Now that you've seen the core workflow, explore the detailed docs for each feature:

- **[Setup & Installation](setup.md)** — package dependencies at a glance
- **[IOC Container](ioc)** — .NET DI, Autofac, mocking, and custom service builders
- **[Builders](builders)** — builder lifecycle, conditional actions, and custom builders
- **[EF Core Builder](builders/ef-builder.md)** — database-specific methods like `Merge`
- **[Data Templates](data-templates)** — reusable, composable data presets
- **[Test Context](test-context)** — sharing state across builders, templates, and tests
- **[Config & Auto Discovery](config-options)** — automatic builder and template registration
- **[BDD Extension](bdd)** — Given/When/Then fluent API
- **[ASP.NET Core](asp-net-core)** — controller integration tests with the full ASP.NET stack
