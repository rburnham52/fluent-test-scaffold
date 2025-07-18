using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Core;

/// <summary>
/// Test Scaffold is the entry point for the Fluent Api.
/// Internally it uses an IOC container that can be used by IOC builds to inject application services.
/// </summary>
public class TestScaffold
{
    public readonly ConfigOptions Options;

    /// <summary>
    /// Provides access to the IOC container.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Provides a places to store data during setup that can be accesses later.
    /// </summary>
    public TestScaffoldContext TestScaffoldContext { get; private set; } = new();

    /// <summary>
    /// Creates an instance of TestScaffold
    /// </summary>
    public TestScaffold()
    {
        Options = ConfigOptions.Default;
    }

    /// <summary>
    /// Creates an instance of TestScaffold with config
    /// </summary>
    public TestScaffold(ConfigOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// Initialises the IOC container with the default .Net Service Builder.
    /// </summary>
    /// <param name="configureServices">Allows you to configure the IOC setup</param>
    public TestScaffold UseIoc(Action<DotnetServiceBuilder>? configureServices = null)
    {
        return UseServiceProviderFactory(new DotnetServiceBuilder(Options), configureServices);
    }

    /// <summary>
    /// Initialises the IOC container with a custom Service Builder.
    /// </summary>
    /// <param name="serviceBuilder">Custom Service Builder than implements IocServiceBuilder</param>
    /// <param name="configureServices">Allows you to configure the IOC setup</param>
    public TestScaffold UseIoc<TContainerBuilder, TServiceBuilder>(
        IocServiceBuilder<TContainerBuilder, TServiceBuilder> serviceBuilder,
        Action<TServiceBuilder>? configureServices = null)
        where TServiceBuilder : IocServiceBuilder<TContainerBuilder, TServiceBuilder>
        where TContainerBuilder : notnull

    {
        return UseServiceProviderFactory(serviceBuilder, configureServices);
    }

    /// <summary>
    /// Resolves and switches the Fluent API to a Builder.
    /// </summary>
    /// <typeparam name="TBuilder">Type of Builder to Resolve</typeparam>
    /// <returns>The resolved builder</returns>
    /// <exception cref="InvalidOperationException">Thrown when builder has not been registered</exception>
    public TBuilder UsingBuilder<TBuilder>() where TBuilder : IBuilder
    {
        if (ServiceProvider is null)
            UseIoc();

        var builder = ServiceProvider!.GetService<TBuilder>();
        if (builder is null)
            throw new InvalidOperationException($"You must Register the Builder with the IOC container before using it");

        return builder;

    }


    /// <summary>
    /// Resolves a service from the IOC container. Will build the IOC container if not already built.
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="T"></typeparam>
    public TestScaffold Resolve<T>(out T service) where T : notnull
    {
        service = Resolve<T>();
        return this;
    }

    /// <summary>
    /// Resolves a service from the IOC container. Will build the IOC container if not already built.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Resolve<T>() where T : notnull
    {
        if (ServiceProvider == null) UseIoc();
        return ServiceProvider!.GetRequiredService<T>();
    }


    /// <summary>
    /// Finds a Data Template and applies it.
    /// </summary>
    /// <param name="templateName">Matched again the the Method Name or Attribute Name Property</param>
    /// <param name="parameters">Params array for optional parameters</param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException">Thrown when no matching DataTemplate method could be found</exception>
    /// <exception cref="InvalidOperationException">Thrown where then was a problem Invoking the matched method</exception>
    public TestScaffold WithTemplate(string templateName, params object[] parameters)
    {
        try
        {
            var dataTemplateService = Resolve<DataTemplateService>();

            var methodInfo = dataTemplateService.FindByName(templateName);
            if (methodInfo.DeclaringType == null)
                throw new InvalidOperationException("Failed to Invoke Data Template. Unknown Declaring Type");
            var instance = Activator.CreateInstance(methodInfo.DeclaringType);
            var paramsArray = (new object?[] { this }).Concat(parameters).ToArray();

            methodInfo.Invoke(instance, paramsArray);

            return this;
        }
        catch (Exception e)
        {
            throw new DataTemplateException(templateName, e);
        }
    }

    /// <summary>
    /// Internal mechanism to allow us to assign an already prepared ServiceProvider to the Test Scaffold.
    /// </summary>
    /// <param name="serviceProvider"></param>
    internal TestScaffold WithServiceProvider(IServiceProvider serviceProvider)
    {
        if (this.ServiceProvider != null)
            throw new InvalidOperationException("A service provider has already been initialised");

        this.ServiceProvider = serviceProvider;

        return this;
    }


    private TestScaffold UseServiceProviderFactory<TContainerBuilder, TServiceBuilder>(
        IocServiceBuilder<TContainerBuilder, TServiceBuilder> serviceBuilder,
        Action<TServiceBuilder>? configureServices = null)
        where TContainerBuilder : notnull
        where TServiceBuilder : IocServiceBuilder<TContainerBuilder, TServiceBuilder>
    {
        if (serviceBuilder is null)
        {
            throw new ArgumentNullException(nameof(serviceBuilder));
        }
        if (ServiceProvider != null) return this;

        if (configureServices != null)
            configureServices((TServiceBuilder)serviceBuilder);

        serviceBuilder.RegisterSingleton(this);
        serviceBuilder.RegisterSingleton(TestScaffoldContext);
        serviceBuilder.RegisterSingleton(DefaultLogger.Logger);
        serviceBuilder.RegisterBuildersWithAutoDiscovery();
        serviceBuilder.RegisterDataTemplatesWithAutoDiscovery();

        ServiceProvider = serviceBuilder.CreateServiceProvider(serviceBuilder.Container);
        return this;
    }
}
