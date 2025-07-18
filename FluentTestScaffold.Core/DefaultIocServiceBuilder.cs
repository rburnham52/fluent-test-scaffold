using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FluentTestScaffold.Core;

/// <summary>
/// Default .net IOC service Builder.
/// </summary>
public class DotnetServiceBuilder : DotnetServiceBuilder<DotnetServiceBuilder>
{
    public DotnetServiceBuilder() : base(ConfigOptions.Default) { }
    public DotnetServiceBuilder(ConfigOptions configOptions) : base(configOptions) { }
}

/// <summary>
/// Default .net IOC Service Builder used to create customer Service Builders.
/// </summary>
/// <typeparam name="TServiceBuilder">Your custom Service Builder type</typeparam>
public class DotnetServiceBuilder<TServiceBuilder> : IocServiceBuilder<IServiceCollection, TServiceBuilder>
    where TServiceBuilder : IocServiceBuilder<IServiceCollection, TServiceBuilder>
{
    public DotnetServiceBuilder(ConfigOptions configOptions) : base(new DefaultServiceProviderFactory(), configOptions) { }

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
        Container.AddTransient<TService>(_ => mock.Object);
        return (this as TServiceBuilder)!;
    }

    /// <summary>
    /// Register a Service as a Singleton with the IOC container.
    /// Used by the Test Scaffold to register itself with the IOC container.
    /// </summary>
    public override void RegisterSingleton<TService>(TService service)
    {
        Container.AddSingleton(_ => service);
    }

    /// <summary>
    /// Register one ot more Services as a Singleton with the IOC container.
    /// Used by the Test Scaffold to register itself with the IOC container.
    /// </summary>
    public override void RegisterSingleton(params Type[] services)
    {
        foreach (var type in services)
        {
            Container.AddSingleton(type);
        }
    }

}
