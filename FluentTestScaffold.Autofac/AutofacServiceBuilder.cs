using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Moq;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core;

/// <summary>
/// Default Autofac Service Builder
/// </summary>
public class AutofacServiceBuilder : AutofacServiceBuilder<AutofacServiceBuilder>
{
    public AutofacServiceBuilder(): base(ConfigOptions.Default) { }
    public AutofacServiceBuilder(ConfigOptions configOptions): base(configOptions) { }
}
/// <summary>
/// Autofac Service Builder used to create customer Service Builders. 
/// </summary>
/// <typeparam name="TServiceBuilder">Your custom Service Builder type</typeparam>
public class AutofacServiceBuilder<TServiceBuilder> : IocServiceBuilder<ContainerBuilder, TServiceBuilder>
    where TServiceBuilder : IocServiceBuilder<ContainerBuilder, TServiceBuilder>
{
    private static readonly ConfigOptions Options = new()
    {
        AutoDiscovery = AutoDiscovery.All,
        Assemblies = new List<Assembly>(){Assembly.GetCallingAssembly()}
    };
    
    public AutofacServiceBuilder() : base(new AutofacServiceProviderFactory(), Options) {}
    
    public AutofacServiceBuilder(ConfigOptions configOptions) : base(new AutofacServiceProviderFactory(), configOptions) {}
    
    /// <summary>
    /// Allows mocking a Service and Registering ith with the container.
    /// </summary>
    /// <param name="config">Allows you to configure the Mock before returning it to be registered in the IOC</param>
    /// <typeparam name="TService">The type that you are mocking</typeparam>
    /// <returns></returns>
    public override TServiceBuilder WithMock<TService>(Func<Mock<TService>, Mock<TService>> config) where TService : class
    {
        var mock = new Mock<TService>();
        config(mock);
        Container.RegisterInstance(mock.Object).As<TService>();
        return (this as TServiceBuilder)! ;
    }

    /// <summary>
    /// Register a Service as a Singleton with the IOC container.
    /// Used by the Test Scaffold to register itself with the IOC container.
    /// </summary>
    public override void RegisterSingleton<TService>(TService service)
    {
        Container.RegisterInstance(service).SingleInstance();
    }

    /// <summary>
    /// Register one ot more Services as a Singleton with the IOC container.
    /// Used by the Test Scaffold to register itself with the IOC container.
    /// </summary>
    public override void RegisterSingleton(params Type[] services)
    {
        foreach (var type in services)
        {
            if (type.IsGenericType)
                Container.RegisterGeneric(type).SingleInstance();
            else
                Container.RegisterType(type).SingleInstance();
        }
    }
}