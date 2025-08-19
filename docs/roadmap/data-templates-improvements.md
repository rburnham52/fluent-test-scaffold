# Data Templates Redesign - Design Document

## Overview

This document outlines the redesign of the Data Templates system to address compile-time safety, IOC integration, and improved developer experience. The new design replaces the current reflection-based approach with a strongly-typed, class-based system.

## Current Problems

1. **Runtime Parameter Mismatch**: Parameters passed as `object[]` with no compile-time validation
2. **Reflection-based Invocation**: Uses `MethodInfo.Invoke` which is error-prone and slow
3. **Limited Composition**: Templates are monolithic methods that can't easily combine
4. **No Type Safety**: Loss of type information for parameters
5. **Poor Error Handling**: Errors only discovered at runtime

## Design Goals

- ✅ **Compile-time Safety**: Method signatures and parameter types validated at build time
- ✅ **IOC Integration**: Templates use dependency injection for services and builders
- ✅ **Simple API**: Clean, intuitive fluent API for template usage
- ✅ **Type Safety**: Full type information preserved throughout the system
- ✅ **Auto-discovery**: Templates automatically discovered and registered with IOC

## New Architecture

### 1. Core Components

#### DataTemplatesAttribute
```csharp
[AttributeUsage(AttributeTargets.Class)]
public class DataTemplatesAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

#### Template Class Structure
```csharp
[DataTemplates(Name = "Seed Data Templates")]
public class SeedDataTemplates
{
    private readonly UserBuilder _userBuilder;
    private readonly CatalogueBuilder _catalogueBuilder;

    public SeedDataTemplates(UserBuilder userBuilder, CatalogueBuilder catalogueBuilder)
    {
        _userBuilder = userBuilder;
        _catalogueBuilder = catalogueBuilder;
    }

    public void ApplyDefaultCatalogue(UserId userId, CatalogueItems catalogueItems)
    {
        // Implementation using injected builders
    }

    public void ApplyUserWithShoppingCart(UserId userId, ShoppingCartType cartType)
    {
        // Implementation using injected builders
    }
}
```

### 2. Fluent API

#### Basic Usage
```csharp
var testScaffold = new TestScaffold()
    .UseIoc()
    .WithTemplate<SeedDataTemplates>(dt => dt.ApplyDefaultCatalogue(userId, catalogueItems));
```

#### Multiple Templates
```csharp
var testScaffold = new TestScaffold()
    .UseIoc()
    .WithTemplate<SeedDataTemplates>(dt => dt.ApplyDefaultCatalogue(userId, catalogueItems))
    .WithTemplate<SeedDataTemplates>(dt => dt.ApplyUserWithShoppingCart(userId, ShoppingCartType.Premium));
```

### 3. Implementation Details

#### Extension Methods
```csharp
public static class TestScaffoldExtensions
{
    public static TestScaffold WithTemplate<TTemplate, T1>(
        this TestScaffold testScaffold, 
        Expression<Func<TTemplate, Action<T1>>> templateSelector,
        T1 param1)
        where TTemplate : class
    {
        var template = testScaffold.Resolve<TTemplate>();
        var compiledSelector = templateSelector.Compile();
        var method = compiledSelector(template);
        method(param1);
        return testScaffold;
    }

    public static TestScaffold WithTemplate<TTemplate, T1, T2>(
        this TestScaffold testScaffold, 
        Expression<Func<TTemplate, Action<T1, T2>>> templateSelector,
        T1 param1, T2 param2)
        where TTemplate : class
    {
        var template = testScaffold.Resolve<TTemplate>();
        var compiledSelector = templateSelector.Compile();
        var method = compiledSelector(template);
        method(param1, param2);
        return testScaffold;
    }

    // Additional overloads for different parameter counts
}
```

#### Auto-discovery Registration
```csharp
public static void RegisterDataTemplatesWithAutoDiscovery(this IServiceCollection services, ConfigOptions configOptions)
{
    if (!configOptions.AutoDiscovery.HasFlag(AutoDiscovery.DataTemplates)) return;
    
    foreach (var assembly in configOptions.Assemblies)
    {
        var templateTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(DataTemplatesAttribute), false).Any())
            .ToArray();
            
        foreach (var templateType in templateTypes)
        {
            services.AddSingleton(templateType);
        }
    }
}
```

## Benefits

### Compile-time Safety
- Method signatures validated at build time
- Parameter types checked during compilation
- No more `MissingMethodException` at runtime
- Full IntelliSense support

### IOC Integration
- Templates resolved from IOC container
- Dependencies properly injected
- No more `Activator.CreateInstance`
- Consistent with existing builder pattern

### Developer Experience
- Clear, readable API
- Refactoring support (rename methods, parameters)
- Compile-time error detection
- Consistent with existing fluent API patterns

## Migration Path

### Breaking Changes
- `[DataTemplate]` attribute replaced with `[DataTemplates]`
- Template methods must be in classes (not standalone methods)
- `WithTemplate(string name, params object[] parameters)` replaced with strongly-typed version

### Migration Steps
1. Replace `[DataTemplate]` with `[DataTemplates]` on classes
2. Move template methods into classes with proper constructors
3. Update `WithTemplate` calls to use new strongly-typed API
4. Remove old reflection-based `DataTemplateService`

## Usage Examples

### Basic Template
```csharp
[Test]
public void ComponentIntegrationTest_UsingDataTemplates_OverAged()
{
    var userId = UserId.New();
    var catalogueItems = new CatalogueItems { Minions, Avengers };

    var testScaffold = new TestScaffold()
        .UseIoc()
        .WithTemplate<SeedDataTemplates>(dt => dt.ApplyDefaultCatalogue(userId, catalogueItems));
}
```

### Complex Template with Dependencies
```csharp
[DataTemplates]
public class AdvancedSeedTemplates
{
    private readonly SeedDataTemplates _basicTemplates;
    private readonly InventoryBuilder _inventoryBuilder;

    public AdvancedSeedTemplates(SeedDataTemplates basicTemplates, InventoryBuilder inventoryBuilder)
    {
        _basicTemplates = basicTemplates;
        _inventoryBuilder = inventoryBuilder;
    }

    public void ApplyFullTestEnvironment(int userCount, CatalogueItems items)
    {
        // Implementation using injected dependencies
    }
}
```

## Future Enhancements

### Potential Enhanced Fluent API
```csharp
// Method chaining within template class
var testScaffold = new TestScaffold()
    .UseIoc()
    .WithTemplate<SeedDataTemplates>()
        .Execute(dt => dt.ApplyDefaultCatalogue(userId, catalogueItems))
        .Execute(dt => dt.ApplyUserWithShoppingCart(userId, ShoppingCartType.Premium))
        .Build();
```

### Additional Considerations
- Async template support
- Template composition and dependency management
- Enhanced error handling and rollback mechanisms
- Template lifecycle hooks
- Parameter validation attributes

## Implementation Priority

1. **Phase 1**: Core strongly-typed template system
2. **Phase 2**: Auto-discovery and IOC integration
3. **Phase 3**: Additional extension method overloads
4. **Phase 4**: Enhanced fluent API (if needed)

## Conclusion

This redesign addresses the core issues of the current system while maintaining simplicity and providing a solid foundation for future enhancements. The strongly-typed approach eliminates runtime errors and provides a much better developer experience through compile-time validation and IntelliSense support.
