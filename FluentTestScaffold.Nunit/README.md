# FluentTestScaffold.Nunit

NUnit testing framework support for FluentTestScaffold.

## Installation

```bash
dotnet add package FluentTestScaffold.Nunit
```

## Documentation

For complete documentation, examples, and usage guides, visit:
**[📚 FluentTestScaffold Documentation](https://github.com/rburnham52/fluent-test-scaffold)**

## What's Included

- NUnit logger support for BDD extensions.

## Quick Start

```csharp
using FluentTestScaffold.Nunit;
using NUnit.Framework;

[TestFixture]
public class MyIntegrationTests
{
    [Test]
    public void TestWithScaffold()
    {
     var testScaffold = new TestScaffold()
            // Will be used to log scenario steps
            .UsingNunit()
            .UseIoc(new AppServicesBuilder());
            
        // Your test logic here
    }
}
```

For detailed examples and advanced usage, see the [main documentation](https://github.com/rburnham52/fluent-test-scaffold). 