# Config Options
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
* DataTemplates - Enables Discovery for DataTemplates
* All - Enables discovery for all

###