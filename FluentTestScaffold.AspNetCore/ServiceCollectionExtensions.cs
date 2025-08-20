using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FluentTestScaffold.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceServiceWithMock<TService>(IServiceCollection services, Mock<TService> mock)
        where TService : class
    {
        var service = services.Single(sd => sd.ServiceType == typeof(TService));
        services.Remove(service);

        services.AddSingleton<TService>(_ => mock.Object);

        return services;
    }
}
