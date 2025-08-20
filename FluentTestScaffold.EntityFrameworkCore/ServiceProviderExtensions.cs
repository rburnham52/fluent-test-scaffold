using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.EntityFrameworkCore;

public static class ServiceProviderExtensions
{
    public static IServiceCollection ReplaceDbContextWithInMemoryProvider<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        RemoveExistingDbContextRegistration<TDbContext>(services);
        RemoveExistingDbContextOptionsRegistration<TDbContext>(services);
        AddInMemoryDbContext<TDbContext>(services);

        return services;
    }

    private static void RemoveExistingDbContextRegistration<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var dbContextDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(TDbContext));
        if (dbContextDescriptor != null)
        {
            services.Remove(dbContextDescriptor);
        }
    }

    private static void RemoveExistingDbContextOptionsRegistration<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var optionsDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (optionsDescriptor != null)
        {
            services.Remove(optionsDescriptor);
        }
    }

    private static void AddInMemoryDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        var inMemoryDatabaseName = Guid.NewGuid().ToString();
        services.AddDbContext<TDbContext>(options => options
            .UseInMemoryDatabase(inMemoryDatabaseName));
    }
}
