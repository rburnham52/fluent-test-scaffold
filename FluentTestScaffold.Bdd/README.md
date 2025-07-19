# FluentTestScaffold.Bdd

Enables you to write BDD (Behavior Driven Development) style test with FluentTestScaffold.

## Installation

```bash
dotnet add package FluentTestScaffold.Bdd
```

## Documentation

For complete documentation, examples, and usage guides, visit:
**[📚 FluentTestScaffold Documentation](https://github.com/rburnham52/fluent-test-scaffold)**

## What's Included

- BDD Style Fluent Api
- Custom BDD style test results logging (currently only supports NUnit)

## Quick Start

```csharp
using FluentTestScaffold.Bdd;

var testScaffold = new TestScaffold()
    .UseIoc(services => {
            services.AddScoped<IAuthService, AuthService>();
    })
    .Build();
    
    
    testScaffold.Scenario("User can add item to shopping cart")
    .Given("a user is authenticated", scaffold => {
        // Setup user authentication
    })
    .When<CartService>("the user adds an item to cart", cartService => {
        // Perform the action
        cartService.AddItem(itemId);
    })
    .Then("the item should be in the cart", scaffold => {
        // Assert the result
        var cart = scaffold.Resolve<ICartService>().GetCart();
        Assert.Contains(itemId, cart.Items);
    });
```

For detailed examples and advanced usage, see the [main documentation](https://github.com/rburnham52/fluent-test-scaffold). 