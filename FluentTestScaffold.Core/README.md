# FluentTestScaffold.Core

Core framework and interfaces for FluentTestScaffold.

## Installation

```bash
dotnet add package FluentTestScaffold.Core
```

## Documentation

For complete documentation, examples, and usage guides, visit:
**[📚 FluentTestScaffold Documentation](https://github.com/rburnham52/fluent-test-scaffold)**

## What's Included

- Core `TestScaffold` class
- Fluent API interfaces
- Builder pattern implementations
- Data template system
- IOC container abstractions

## Quick Start

```csharp
using FluentTestScaffold.Core;

var testScaffold = new TestScaffold()
    .UseIoc(services => {
        // Configure your services
    })
    .Build();
```

For detailed examples and advanced usage, see the [main documentation](https://github.com/rburnham52/fluent-test-scaffold). 