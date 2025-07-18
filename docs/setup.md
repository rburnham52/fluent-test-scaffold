# Setup

## Package Dependencies

```mermaid
%%{init: {'theme':'base', 'themeVariables': {'primaryColor':'#ffffff','primaryTextColor':'#000000','primaryBorderColor':'#000000','lineColor':'#000000','secondaryColor':'#f0f0f0','tertiaryColor':'#ffffff'}}}%%
flowchart TB
    subgraph "Core Package"
        CORE[FluentTestScaffold.Core<br/>Required Base Package]
    end
    subgraph "IOC Extensions"
        AUTOFAC[FluentTestScaffold.Autofac<br/>Autofac Container Support]
    end
    subgraph "Database Extensions"
        EF[FluentTestScaffold.EntityFrameworkCore<br/>EF Core Builder Support]
    end
    subgraph "Testing Extensions"
        BDD[FluentTestScaffold.Bdd<br/>BDD Style API]
        ASPNET[FluentTestScaffold.AspNetCore<br/>Controller Integration Tests]
    end
    CORE --> AUTOFAC
    CORE --> EF
    CORE --> BDD
    CORE --> ASPNET
    
    style CORE fill:#e3f2fd,stroke:#000,stroke-width:2px,color:#000
    style AUTOFAC fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
    style EF fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style BDD fill:#fff3e0,stroke:#000,stroke-width:2px,color:#000
    style ASPNET fill:#ffebee,stroke:#000,stroke-width:2px,color:#000
```

Install the core Library.

[Install `FluentTestScaffold.Core`](https://github.com/rburnham52/fluent-test-scaffold/pkgs/nuget/FluentTestScaffold.Core)

### Autofac Ioc support
To enable Autofac ioc Container

[Install `FluentTestScaffold.Autofac`](https://github.com/rburnham52/fluent-test-scaffold/pkgs/nuget/FluentTestScaffold.Autofac)

### Entity Framework Core support
To enable the use of EF core builders.

[Install `FluentTestScaffold.EntityFrameworkCore`](https://github.com/rburnham52/fluent-test-scaffold/pkgs/nuget/FluentTestScaffold.EntityFrameworkCore)

