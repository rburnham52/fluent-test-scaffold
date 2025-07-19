# FluentTestScaffold.Autofac

Autofac DI container support for FluentTestScaffold.

## Installation

```bash
dotnet add package FluentTestScaffold.Autofac
```

## Documentation

For complete documentation, examples, and usage guides, visit:
**[📚 FluentTestScaffold Documentation](https://github.com/rburnham52/fluent-test-scaffold)**

## What's Included

- Autofac container integration
- Service registration extensions
- Lifetime scope management
- Dependency resolution helpers
- Container builder utilities

## Quick Start

```csharp
using FluentTestScaffold.Autofac;

var testScaffold = new TestScaffold()
    .UseAutofac(ctx =>
    {
        ctx.Container.Register<IAuthService>(_ => mockAuthService.Object).SingleInstance();
        ctx.Container.RegisterType<UserRequestContext>().As<IUserRequestContext>().SingleInstance();
    });
```

For detailed examples and advanced usage, see the [main documentation](https://github.com/rburnham52/fluent-test-scaffold). 