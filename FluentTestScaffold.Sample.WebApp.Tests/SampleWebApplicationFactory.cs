using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace FluentTestScaffold.Sample.WebApp.Tests;

public class SampleWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();
        });
    }
}