# Config Options & Auto Discovery

Config Options allow you to configure the behaviour of the Test Scaffold.

## Auto Discovery

```mermaid
%%{init: {'theme':'base', 'themeVariables': {'primaryColor':'#ffffff','primaryTextColor':'#000000','primaryBorderColor':'#000000','lineColor':'#000000','secondaryColor':'#f0f0f0','tertiaryColor':'#ffffff'}}}%%
flowchart TD
    START[TestScaffold with ConfigOptions] --> SCAN[Scan Configured Assemblies]

    SCAN --> BUILDERS[Find Classes Inheriting Builder]
    SCAN --> TEMPLATES[Find Classes with DataTemplates Attribute]

    BUILDERS --> REG_B[Register Builders with IOC]
    TEMPLATES --> REG_T[Register Template Classes with IOC]

    REG_B --> AVAILABLE[Available via UsingBuilder T]
    REG_T --> AVAILABLE_T[Available via WithTemplate T]

    subgraph "Auto Discovery Options"
        NONE[AutoDiscovery.None]
        BUILD[AutoDiscovery.Builders]
        DATA[AutoDiscovery.DataTemplates]
        ALL[AutoDiscovery.All]
    end
    style START fill:#e3f2fd,stroke:#000,stroke-width:2px,color:#000
    style SCAN fill:#fff3e0,stroke:#000,stroke-width:2px,color:#000
    style BUILDERS fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style TEMPLATES fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
    style ALL fill:#ffebee,stroke:#000,stroke-width:2px,color:#000
    style NONE fill:#f5f5f5,stroke:#000,stroke-width:2px,color:#000
    style BUILD fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style DATA fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
```

You can override the default configuration for the Test Scaffold by providing a custom config.
```csharp
new TestScaffold(new ConfigOptions());
```

## Assemblies
A list of Assemblies used during Auto Discovery

*Default:* `AppDomain.CurrentDomain.GetAssemblies()`

## AutoDiscovery
Auto Discovery is a feature that allows you to automatically register all the Builders and/or Data Templates in the specified assemblies.

*Default:* `AutoDiscovery.All`

```csharp
new TestScaffold(new ConfigOptions() {AutoDiscovery = AutoDiscovery.None})
    .UseIoc();
```

During IOC registration the Auto Discovery options will be used to Register any Builders and Data Templates found in the given assemblies

### AutoDiscovery Options
* None - Disables Auto Discovery
* Builders - Enables Discovery for Builders
* DataTemplates - Enables Discovery for DataTemplates (classes marked with `[DataTemplates]` attribute)
* All - Enables discovery for all

### Data Templates Auto Discovery

When `AutoDiscovery.DataTemplates` is enabled, the system will:

1. Scan all configured assemblies for classes marked with the `[DataTemplates]` attribute
2. Register each template class as a singleton in the IOC container
3. Make them available for use with the `WithTemplate<TTemplate>()` method

```csharp
// Enable data template auto discovery
var config = new ConfigOptions 
{
    AutoDiscovery = AutoDiscovery.DataTemplates,
    Assemblies = new List<Assembly> { typeof(MyTemplates).Assembly }
};

var testScaffold = new TestScaffold(config).UseIoc();
```
