<a name='assembly'></a>

# FluentTestScaffold.Core

## Contents

- [Builder](#T-FluentTestScaffold-Core-Builder 'FluentTestScaffold.Core.Builder')
    - [#ctor(testScaffold)](#M-FluentTestScaffold-Core-Builder-#ctor-FluentTestScaffold-Core-TestScaffold- 'FluentTestScaffold.Core.Builder.#ctor(FluentTestScaffold.Core.TestScaffold)')
    - [Build()](#M-FluentTestScaffold-Core-Builder-Build 'FluentTestScaffold.Core.Builder.Build')
    - [Enqueue(action)](#M-FluentTestScaffold-Core-Builder-Enqueue-System-Action- 'FluentTestScaffold.Core.Builder.Enqueue(System.Action)')
- [TestScaffold](#T-FluentTestScaffold-Core-TestScaffold 'FluentTestScaffold.Core.TestScaffold')
    - [#ctor()](#M-FluentTestScaffold-Core-TestScaffold-#ctor 'FluentTestScaffold.Core.TestScaffold.#ctor')
    - [BuildServiceProvider()](#M-FluentTestScaffold-Core-TestScaffold-BuildServiceProvider 'FluentTestScaffold.Core.TestScaffold.BuildServiceProvider')
    - [Resolve\`\`1(service)](#M-FluentTestScaffold-Core-TestScaffold-Resolve``1-``0@- '
      FluentTestScaffold.Core.TestScaffold.Resolve``1(``0@)')
    - [Resolve\`\`1()](
      #M-FluentTestScaffold-Core-TestScaffold-Resolve``1 'FluentTestScaffold.Core.TestScaffold.Resolve``1')
    - [SetServiceProvider(serviceProvider)](#M-FluentTestScaffold-Core-TestScaffold-SetServiceProvider-System-IServiceProvider- 'FluentTestScaffold.Core.TestScaffold.SetServiceProvider(System.IServiceProvider)')

<a name='T-FluentTestScaffold-Core-Builder'></a>

## Builder `type`

##### Namespace

FluentTestScaffold.Core

##### Summary

Base Builder Class used to add context to the Test Scaffold.

<a name='M-FluentTestScaffold-Core-Builder-#ctor-FluentTestScaffold-Core-TestScaffold-'></a>

### #ctor(testScaffold) `constructor`

##### Summary

Builder Constructor

##### Parameters

| Name         | Type                                                                                                                   | Description |
|--------------|------------------------------------------------------------------------------------------------------------------------|-------------|
| testScaffold | [FluentTestScaffold.Core.TestScaffold](#T-FluentTestScaffold-Core-TestScaffold 'FluentTestScaffold.Core.TestScaffold') |             |

<a name='M-FluentTestScaffold-Core-Builder-Build'></a>

### Build() `method`

##### Summary

Build the current builder actions and return the TestScaffold context.

##### Returns

##### Parameters

This method has no parameters.

<a name='M-FluentTestScaffold-Core-Builder-Enqueue-System-Action-'></a>

### Enqueue(action) `method`

##### Summary

Enqueue an action to be applied when Build is called.

##### Parameters

| Name   | Type                                                                                                                    | Description |
|--------|-------------------------------------------------------------------------------------------------------------------------|-------------|
| action | [System.Action](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action') |             |

<a name='T-FluentTestScaffold-Core-TestScaffold'></a>

## TestScaffold `type`

##### Namespace

FluentTestScaffold.Core

##### Summary

Test Scaffold is the entry point for the Fluent Api.
Internally it uses an IOC container that can be used by IOC builds to inject application services.

<a name='M-FluentTestScaffold-Core-TestScaffold-#ctor'></a>

### #ctor() `constructor`

##### Summary

Creates an instance of TestScaffold and uses the default IOC

##### Parameters

This constructor has no parameters.

<a name='M-FluentTestScaffold-Core-TestScaffold-BuildServiceProvider'></a>

### BuildServiceProvider() `method`

##### Summary

Builds the default .net Service Provider.

##### Parameters

This method has no parameters.

<a name='M-FluentTestScaffold-Core-TestScaffold-Resolve``1-``0@-'></a>

### Resolve\`\`1(service) `method`

##### Summary

Resolves a service from the IOC container

##### Returns

##### Parameters

| Name    | Type                     | Description |
|---------|--------------------------|-------------|
| service | [\`\`0@](#T-``0@ '``0@') |             |

##### Generic Types

| Name | Description |
|------|-------------|
| T    |             |

<a name='M-FluentTestScaffold-Core-TestScaffold-Resolve``1'></a>

### Resolve\`\`1() `method`

##### Summary

Helper method to resolve a type from ServiceProvider.

##### Returns

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
|------|-------------|
| T    |             |

##### Exceptions

| Name                                                                                                                                                                    | Description |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------|
| [System.NullReferenceException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.NullReferenceException 'System.NullReferenceException') |             |

<a name='M-FluentTestScaffold-Core-TestScaffold-SetServiceProvider-System-IServiceProvider-'></a>

### SetServiceProvider(serviceProvider) `method`

##### Summary

Allows providing a custom Service Provider to be used .

##### Parameters

| Name            | Type                                                                                                                                                  | Description                               |
|-----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------|
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | Custom implementation of IServiceProvider |
