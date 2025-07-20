using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Sample.WebApp.Tests;

public class SampleWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string SharedDatabaseName = Guid.NewGuid().ToString();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var internalDbContext = services.SingleOrDefault(sd => sd.ServiceType == typeof(TestDbContext));
            if (internalDbContext != null)
                services.Remove(internalDbContext);

            var internalDbContextOptions = services.SingleOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<TestDbContext>));
            if (internalDbContextOptions != null)
                services.Remove(internalDbContextOptions);

            services.AddDbContext<TestDbContext>(o => o
                .UseInMemoryDatabase(SharedDatabaseName)
            );
        });
    }
}
