namespace FluentTestScaffold.Core;

public interface IIocServiceServiceCollection<TContainer> : IIocServiceProviderFactory
{
    TContainer RegisterScoped<TService>()
        where TService : class;

    TContainer RegisterScoped<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class;

    TContainer RegisterScopedAs<TService, TInterface>()
        where TService : class, TInterface
        where TInterface : class;

    TContainer RegisterScopedAs<TInterface>(Func<IServiceProvider, object> implementationFactory)
        where TInterface : class;

    TContainer RegisterTransient<TService>()
        where TService : class;

    TContainer RegisterTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class;

    TContainer RegisterTransientAs<TService, TInterface>()
        where TService : class, TInterface
        where TInterface : class;

    TContainer RegisterTransientAs<TInterface>(Func<IServiceProvider, object> implementationFactory)
        where TInterface : class;

    TContainer RegisterSingleton<TService>()
        where TService : class;

    TContainer RegisterSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class;

    TContainer RegisterSingletonAs<TService, TInterface>()
        where TService : class, TInterface
        where TInterface : class;

    TContainer RegisterSingletonAs<TInterface>(Func<IServiceProvider, object> implementationFactory)
        where TInterface : class;
}

public interface IIocServiceProviderFactory
{
    public IServiceProvider GetServiceProvider();
}