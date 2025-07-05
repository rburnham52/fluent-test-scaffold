<a name='assembly'></a>
# FluentTestScaffold.EntityFrameworkCore

## Contents

- [EfCoreBuilder\`1](#T-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`1 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`1')
  - [#ctor(serviceProvider)](#M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`1-#ctor-System-IServiceProvider- 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`1.#ctor(System.IServiceProvider)')
- [EfCoreBuilder\`2](#T-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`2')
  - [#ctor(serviceProvider)](#M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-#ctor-System-IServiceProvider- 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`2.#ctor(System.IServiceProvider)')
  - [WithRange\`\`1(entities)](#M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-WithRange``1-System-Collections-Generic-IEnumerable{``0}- 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`2.WithRange``1(System.Collections.Generic.IEnumerable{``0})')
  - [With\`\`1(entity)](#M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-With``1-``0- 'FluentTestScaffold.EntityFrameworkCore.EfCoreBuilder`2.With``1(``0)')

<a name='T-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`1'></a>
## EfCoreBuilder\`1 `type`

##### Namespace

FluentTestScaffold.EntityFrameworkCore

##### Summary

Base builder for builder up an entity framework DbContext

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TDbContext | The DBContext to build |

<a name='M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`1-#ctor-System-IServiceProvider-'></a>
### #ctor(serviceProvider) `constructor`

##### Summary

Base builder for builder up an entity framework DbContext

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | Will be injected from the TestScaffold Ioc |

<a name='T-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2'></a>
## EfCoreBuilder\`2 `type`

##### Namespace

FluentTestScaffold.EntityFrameworkCore

##### Summary

Base builder for custom builder implementations.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TDbContext | The DBContext to build |
| TBuilder | The type of the Custom Builder |

<a name='M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-#ctor-System-IServiceProvider-'></a>
### #ctor(serviceProvider) `constructor`

##### Summary

Base builder for builder up an entity framework DbContext

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | Will be injected from the TestScaffold Ioc |

<a name='M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-WithRange``1-System-Collections-Generic-IEnumerable{``0}-'></a>
### WithRange\`\`1(entities) `method`

##### Summary

Adds a collection of entities to the DbContext

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| entities | [System.Collections.Generic.IEnumerable{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{``0}') | The DbContext Entities to add |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TEntity | The Entity Type |

<a name='M-FluentTestScaffold-EntityFrameworkCore-EfCoreBuilder`2-With``1-``0-'></a>
### With\`\`1(entity) `method`

##### Summary

Adds a single entity to the DbContext

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| entity | [\`\`0](#T-``0 '``0') | The DbContext Entity to add |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TEntity | The Entities Type |
