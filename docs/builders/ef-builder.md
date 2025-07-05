## Entity Framework Builder

The EF Builder allows you to initialize your database using Entity Framework Core.

The default EfCoreBuilder provides some standard method to setup your DBContext.
Ensure the DBContext is registered into the IOC Container before use. 
```csharp
var dbContext = TestDbContextFactory.Create();
var userId = Guid.Parse("65579043-8112-480C-A885-C6157947F0F3");

new TestScaffold()
    .UseIoc(ctx =>
    {
        ctx.Container.RegisterSingleton(_ => dbContext);
        ctx.RegisterBuilder<EfCoreBuilder<TestDbContext>>();
    })
    .UsingBuilder<EfCoreBuilder<TestDbContext>>()
    .With(new User(
        id: userId,
        email: "Bob@test.com",
        password: "",
        name: "Bob",
        dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-15))
    ))
    .Build();
```
 
See Tests for more examples.
### Using EF Builders

The EF Builder inherits key methods like `With`, `WithRange`, `WithRange` and `Build` from the `Builder` base class.

There are some EF specific methods that can be used to interact with the DBContext

#### Merge
The `Merge` method will update an existing entity based on the Pkey of the entity.
```csharp
new TestScaffold()
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
        
var updatedItem = new Item { Id = itemId, Title = "Updated Item", Price = 30 };
testScaffold
    .UsingBuilder<InventoryBuilder>()
    .Merge(updatedItem)
    .Build();
```

### Custom Builder Methods

Custom Builders can be created to  that contain predefined data structures that allow you to quickly insert common data
In this example we now a Shopping Cart requires a UserId so we can create a builder method for it to quickly setup shopping carts

```csharp
new TestScaffold()
      .UseIoc(ctx =>
      {
          ctx.RegisterBuilder<InventoryBuilder>();
          ctx.Container.RegisterSingleton(_ => dbContext);
      })
      .UsingBuilder<InventoryBuilder>()
      .WithDefaultCatalogue()
      .With(new User(
          id: userId,
          email: "Bob@test.com",
          password: "",
          name: "Bob",
          dateOfBirth: DateOnly.FromDateTime(DateTime.Now.AddYears(-15))
      ))
      .WithShoppingCart(userId)
      .Build();
```

Implement a EF core builder by inheriting `EfCoreBuilder<TDbContext, TBuilder>`. 
`TBuilder` should be the same type as your custom builder to ensure the Fluent Api continues with your Custom Builders Fluent API

**It's convention to start builder methods with the `With` prefix**
```csharp
public class InventoryBuilder : EfCoreBuilder<TestDbContext, InventoryBuilder>
{
    public InventoryBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Adds a Shopping cart for the User
    /// </summary>
    public InventoryBuilder WithShoppingCart(Guid userId)
    {
        With(new ShoppingCart()
        {
            Id = Guid.NewGuid(),
            UserId = userId
        });

        return this;
    }
    
        /// <summary>
    /// Adds a set of sample Items to the DBContext
    /// </summary>
    public InventoryBuilder WithDefaultCatalogue()
    {
        WithRange(new List<Item>()
        {
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Minions, Price = 21},
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Avengers, Price = 24},
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.DeadPool, Price = 14, AgeRestriction = 15}
        });

        return this;
    }
}
```
