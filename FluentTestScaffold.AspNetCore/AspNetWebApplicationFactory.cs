using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FluentTestScaffold.AspNetCore;

public class AspNetWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly TestScaffold _testScaffold;
    private readonly Action<IServiceCollection>? _configureServices;
    private readonly Action<IHostBuilder>? _configureHost;

    public AspNetWebApplicationFactory(
        TestScaffold testScaffold,
        Action<IServiceCollection>? configureServices = null,
        Action<IHostBuilder>? configureHost = null)
    {
        _testScaffold = testScaffold;
        _configureServices = configureServices;
        _configureHost = configureHost;
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

    /// <summary>
    /// Invokes the <c>configureHost</c> callback (if provided) then builds and starts the host.
    /// The callback runs after all app-level ConfigureContainer callbacks have been registered
    /// but before the host is built, giving it "last wins" override semantics.
    /// </summary>
    /// <remarks>
    /// Subclasses overriding this method MUST call <c>base.CreateHost(builder)</c> to preserve
    /// both the configureHost callback and the host start sequence. Omitting the base call
    /// will silently skip configureHost and cause a deadlock (the host never starts).
    /// </remarks>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        _configureHost?.Invoke(builder);
        return base.CreateHost(builder);
    }
}
