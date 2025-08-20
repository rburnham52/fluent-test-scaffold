using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Core;

internal interface IServiceFactoryAdapter
{
    object CreateBuilder(IServiceCollection services);

    IServiceProvider CreateServiceProvider(object containerBuilder);
}
