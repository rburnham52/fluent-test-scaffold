using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FluentTestScaffold.Core;

public abstract class IocServiceBuilder<TContainerBuilder, TServiceBuilder> : IServiceProviderFactory<TContainerBuilder>
    where TContainerBuilder : notnull
    where TServiceBuilder : IocServiceBuilder<TContainerBuilder, TServiceBuilder>
{
    private readonly IServiceProviderFactory<TContainerBuilder> _serviceProviderFactory;
    private readonly ConfigOptions _configOptions;

    /// <summary>
    /// Provides access to the IOC container.
    /// </summary>
    public TContainerBuilder Container { get; }

    protected IocServiceBuilder(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory, ConfigOptions configOptions)
    {
        _serviceProviderFactory = serviceProviderFactory;
        _configOptions = configOptions;
        Container = serviceProviderFactory.CreateBuilder(new ServiceCollection());
    }

    /// <summary>
    /// Allows mocking a Service and Registering ith with the container.
    /// </summary>
    /// <param name="config">Allows you to configure the Mock before returning it to be registered in the IOC</param>
    /// <typeparam name="TService">The type that you are mocking</typeparam>
    /// <returns></returns>
    public abstract TServiceBuilder WithMock<TService>(Func<Mock<TService>, Mock<TService>> config)
        where TService : class;

    /// <summary>
    /// Called internally to register the TestScaffold, Builders and Data Templates with the IOC container.
    /// </summary>
    public abstract void RegisterSingleton<TSerivce>(TSerivce service) where TSerivce : class;

    /// <summary>
    /// Called internally to register the TestScaffold, Builders and Data Templates with the IOC container.
    /// </summary>
    public abstract void RegisterSingleton(params Type[] services);


    /// <summary>
    /// Registers all builders in the specified assemblies to the IOC container if EnableAutoDiscovery is true.
    /// </summary>
    public void RegisterBuildersWithAutoDiscovery()
    {
        if (!_configOptions.AutoDiscovery.HasFlag(AutoDiscovery.Builders)) return;
        foreach (var assembly in _configOptions.Assemblies)
        {
            var builderTypes = assembly.GetTypes()
                .Where(t => t.IsAssignableToGenericType(typeof(Builder<>)))
                .Where(t => t != typeof(Builder<>)) // ignore base builder for internal testing
                .ToArray();
            if (builderTypes.Length > 0)
                RegisterSingleton(builderTypes);
        }
    }

    /// <summary>
    /// Registers all data templates in the specified assemblies to the IOC container if EnableAutoDiscovery is true.
    /// </summary>
    public void RegisterDataTemplatesWithAutoDiscovery()
    {
        var dataTemplateMethods = TestScaffoldServiceRegistration.GetDataTemplateMethods(_configOptions);
        var dataTemplateService = new DataTemplateService(dataTemplateMethods);
        RegisterSingleton(dataTemplateService);
    }


    /// <summary>
    /// Called internally to construct a IServiceProvider
    /// </summary>
    /// <param name="containerBuilder"></param>
    /// <returns></returns>
    public IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder)
    {
        return _serviceProviderFactory.CreateServiceProvider(containerBuilder);
    }

    /// <summary>
    /// IServiceProviderFactory implementation to create a Container Builder
    /// </summary>
    public TContainerBuilder CreateBuilder(IServiceCollection services)
    {
        return Container;
    }
}
