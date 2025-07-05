using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.EntityFrameworkCore;

public static class ServiceProviderExtensions
{
    public static IServiceCollection ReplaceDbContextWithInMemoryProvider<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var internalDbContext = services.Single(sd => sd.ServiceType == typeof(TDbContext));
        services.Remove(internalDbContext);

        var internalDbContextOptions = services.Single(sd => sd.ServiceType == typeof(DbContextOptions<TDbContext>));
        services.Remove(internalDbContextOptions);

        var inmemoryDbContextName = Guid.NewGuid().ToString();
        services.AddDbContext<TDbContext>(o => o
            .UseInMemoryDatabase(inmemoryDbContextName)
        );

        return services;
    }
}