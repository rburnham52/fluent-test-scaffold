# IOC
Fluent Test Scaffold uses the .Net IOC Container by default to register and resolve builders and their dependencies.

## IOC Container Flow

```mermaid
%%{init: {'theme':'base', 'themeVariables': {'primaryColor':'#ffffff','primaryTextColor':'#000000','primaryBorderColor':'#000000','lineColor':'#000000','secondaryColor':'#f0f0f0','tertiaryColor':'#ffffff'}}}%%
flowchart TB
    subgraph "Service Registration"
        TS[TestScaffold] --> CHOOSE{IOC Choice}
        CHOOSE -->|Default| DOTNET[UseIoc<br/>.NET Container]
        CHOOSE -->|Autofac| AUTOFAC[UseAutofac<br/>Autofac Container]
        
        DOTNET --> REGISTER1[Register Services<br/>AddSingleton/AddTransient]
        AUTOFAC --> REGISTER2[Register Services<br/>RegisterType/Register]
        
        REGISTER1 --> AUTO1[Auto-Register Builders]
        REGISTER2 --> AUTO2[Auto-Register Builders]
        
        AUTO1 --> BUILD1[Build ServiceProvider]
        AUTO2 --> BUILD2[Build ServiceProvider]
    end
    
    subgraph "Service Resolution"
        BUILD1 --> SP[IServiceProvider]
        BUILD2 --> SP
        
        SP --> RESOLVE[Resolve T]
        SP --> BUILDER[UsingBuilder T]
        
        RESOLVE --> SERVICE[Your Service]
        BUILDER --> BUILDERINSTANCE[Builder Instance]
    end
    
    style DOTNET fill:#e3f2fd,stroke:#000,stroke-width:2px,color:#000
    style AUTOFAC fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
    style SP fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style SERVICE fill:#fff3e0,stroke:#000,stroke-width:2px,color:#000
    style TS fill:#e3f2fd,stroke:#000,stroke-width:2px,color:#000
    style CHOOSE fill:#fff3e0,stroke:#000,stroke-width:2px,color:#000
    style BUILDERINSTANCE fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
```

The IOC can be used to register and construct any services under test.
The container is built after calling UseIoc which takes an optional parameter that exposes a Service Builder.
The Service Builder exposes the underlying IOC's Container Builder & Provides some helpful methods to register Builders. 

## Setup
The default .net IOC implementation requires a reference to `Microsoft.Extensions.DependencyInjection`

```csharp
var testScaffold = new TestScaffold()
    .UseIoc(ctx => ctx.Container.AddSingleton<IMockService, MockService>());
```

## Resolving Services
The `TestScaffold` exposes some helper methods to resolve services from the IOC container
```csharp
var testScaffold = new TestScaffold()
    .UseIoc(ctx => ctx.Container.AddTransient<MockService>());

var service = testScaffold.Resolve<MockService>();

/// or using fluent api

MyService? service = null;
new TestScaffold()
    .UseIoc(ctx =>
    {   ctx.RegisterBuilder<MyBuilder>();
        ctx.Container.AddTransient<MyService>();
    })
    .Resolve<MyService>(out service)
    .UsingBuilder<MyBuilder>();
```

## Mocking Services
Fluent Test Scaffold supports the use of the Mocking Library [Moq](https://github.com/devlooped/moq)

```csharp
var timeService = new TestScaffold()
          .UseIoc(serviceBuilder =>
          {
              serviceBuilder.WithMock<ITimeService>(mock =>
              {
                  mock.Setup(c => c.GetTime()).Returns(TimeOnly.Parse(timeString, CultureInfo.CurrentCulture));
                  return mock;
              });
          })
          .Resolve<ITimeService>();
```

## Autofac Support
To use Autofac for the Ioc container, install `FluentTestScaffold.Autofac` and call `UseAutofac` on the `TestScaffold` to replace the default IOC implementation

```csharp
var testScaffold = new TestScaffold()
    .UseAutofac(ctx =>
    {
        ctx.Container.Register<IAuthService>(_ => mockAuthService.Object).SingleInstance();
        ctx.Container.RegisterType<UserRequestContext>().As<IUserRequestContext>().SingleInstance();
    });
```

## Service Builder
When calling UseIoc, the constructor func injects a Service Builder that wraps the IOC's Container Builder and also exposes some helper methods to Register Builders and the Test Scaffold with the IOC container.

### Container
The container prop exposes the underlying IOC's Container Builder.

```csharp
var testScaffold = new TestScaffold()
       .UseIoc(ctx =>
       {
           ctx.Container.AddSingleton(_ => dbContext);
           ctx.RegisterBuilders(typeof(MockBuilder), typeof(InventoryBuilder));
       });
```

### Custom Service Builders

```mermaid
%%{init: {'theme':'base', 'themeVariables': {'primaryColor':'#ffffff','primaryTextColor':'#000000','primaryBorderColor':'#000000','lineColor':'#000000','secondaryColor':'#f0f0f0','tertiaryColor':'#ffffff'}}}%%
classDiagram
    class IocServiceBuilder {
        <<abstract>>
        +Container TContainer
        +RegisterBuilder T()
        +RegisterSingleton T()
        +CreateServiceProvider()
    }
    
    class DotnetServiceBuilder {
        +Container IServiceCollection
        +RegisterAppServices()
    }
    
    class AutofacServiceBuilder {
        +Container ContainerBuilder
        +RegisterAppServices()
    }
    
    class AppServicesBuilder {
        +RegisterAppServices()
    }
    
    class AutofacAppServicesBuilder {
        +RegisterAppServices()
    }
    
    IocServiceBuilder <|-- DotnetServiceBuilder
    IocServiceBuilder <|-- AutofacServiceBuilder
    DotnetServiceBuilder <|-- AppServicesBuilder
    AutofacServiceBuilder <|-- AutofacAppServicesBuilder
    
    style IocServiceBuilder fill:#e3f2fd,stroke:#000,stroke-width:2px,color:#000
    style DotnetServiceBuilder fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style AutofacServiceBuilder fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
    style AppServicesBuilder fill:#e8f5e8,stroke:#000,stroke-width:2px,color:#000
    style AutofacAppServicesBuilder fill:#f3e5f5,stroke:#000,stroke-width:2px,color:#000
```

Custom Service Builders allow you to combine common blocks of registered services making IOC setup simpler.

You can create a custom service builder by inheriting one of the IOC providers Service Builder base classes

* `DotnetServiceBuilder<T>` - .Net's base service builder. (Default IOC)
* `AutofacServiceBuilder<T>` - Autofac's base service builder.

The generic type supplied `T` is the type of your Service Builder and allows for strong typing in the IOC constructor func

#### .Net Service Builder
```csharp
public class AppServicesBuilder : DotnetServiceBuilder<AppServicesBuilder>
{
    public void RegisterAppServices()
    {
        Container.AddSingleton(_ => TestDbContextFactory.Create());
        Container.AddTransient<IUserRequestContext, UserRequestContext>();
        Container.AddTransient<IAuthService, AuthService>();
        Container.AddTransient<ShoppingCart>();
        RegisterBuilder<InventoryBuilder>();
    }
    
}
```

#### Autofac Service Builder

```csharp
public class AutofacAppServicesBuilder : AutofacServiceBuilder<AutofacAppServicesBuilder>
{
    public void RegisterAppServices()
    {
        Container.Register<TestDbContext>(_ => TestDbContextFactory.Create()).SingleInstance();
        Container.RegisterType<UserRequestContext>().As<IUserRequestContext>();
        Container.RegisterType<AuthService>().As<IAuthService>();
        Container.RegisterType<ShoppingCart>();
        RegisterBuilder<InventoryBuilder>();
    }
}
```
#### Usage

To use a custom Service Builder, provide is as a fist parameter to the UseIoc method. 

```csharp
// Default IOC
var testScaffold = new TestScaffold()
     .UseIoc(
         new DefaultIocAppServicesBuilder(),
         ctx => ctx.RegisterAppServices());
     
// Autofac IOC
var testScaffold = new TestScaffold()
         .UseAutofac(
             new AutofacAppServicesBuilder(),
             ctx => ctx.RegisterAppServices()));
```
