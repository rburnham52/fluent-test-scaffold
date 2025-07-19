# FluentTestScaffold.EntityFrameworkCore

Entity Framework Core support for FluentTestScaffold.

The EF Builder allows you to initialize your database using Entity Framework Core.

## Installation

```bash
dotnet add package FluentTestScaffold.EntityFrameworkCore
```

## Documentation

For complete documentation, examples, and usage guides, visit:
**[📚 FluentTestScaffold Documentation](https://github.com/rburnham52/fluent-test-scaffold)**

## What's Included

- Entity Framework Core integration
- Base EfCoreBuilder for extending the Fluent API to build your DbContext
- Data Templates for seeding common actions

## Quick Start

```csharp
using FluentTestScaffold.EntityFrameworkCore;

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

For detailed examples and advanced usage, see the [main documentation](https://github.com/rburnham52/fluent-test-scaffold). 