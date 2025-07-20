using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.AspNetCore;

internal class AspNetWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly TestScaffold _testScaffold;
    private readonly Action<IServiceCollection>? _configureServices;

    public AspNetWebApplicationFactory(TestScaffold testScaffold, Action<IServiceCollection>? configureServices = null)
    {
        _testScaffold = testScaffold;
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddTestScaffoldServices(_testScaffold);
            services.AddSingleton<WebApplicationFactory<TEntryPoint>>(this);
            _configureServices?.Invoke(services);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
