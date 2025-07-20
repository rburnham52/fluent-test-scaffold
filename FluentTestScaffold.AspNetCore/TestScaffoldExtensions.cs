using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.AspNetCore;

public static class TestScaffoldExtensions
{
    public static TestScaffold UseAspNet<TEntryPoint>(
        this TestScaffold testScaffold,
        Action<IServiceCollection>? configureServices = null)
        where TEntryPoint : class
    {
        var factory = new AspNetWebApplicationFactory<TEntryPoint>(testScaffold, configureServices);
        return testScaffold.WithServiceProvider(factory.Services);
    }

    public static TestScaffold UseAspNet<TWebApplicationFactory, TEntryPoint>(
        this TestScaffold testScaffold,
        TWebApplicationFactory webApplicationFactory,
        Action<IServiceCollection>? configureServices = null)
        where TWebApplicationFactory : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        var enhancedFactory = webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddTestScaffoldServices(testScaffold);
                services.AddSingleton<WebApplicationFactory<TEntryPoint>>(webApplicationFactory);
                configureServices?.Invoke(services);
            });
        });

        return testScaffold.WithServiceProvider(enhancedFactory.Services);
    }

    public static HttpClient GetWebApplicationHttpClient<TWebApplicationFactory, TEntry>(this TestScaffold testScaffold)
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        if (testScaffold.ServiceProvider == null)
            throw new InvalidOperationException("A call to testScaffold.UseAspNet<TEntryPoint>() is required to initialise a web application factory before a HttpClient can be created");

        var webApplicationFactory = testScaffold.ServiceProvider.GetService<WebApplicationFactory<TEntry>>();
        if (webApplicationFactory == null)
            throw new InvalidOperationException("No WebApplicationFactory found in service provider. Ensure UseAspNet was called properly.");

        var key = CreateHttpClientKey<TWebApplicationFactory, TEntry>();

        if (!testScaffold.TestScaffoldContext.TryGetValue<HttpClient>(key, out var httpClient))
        {
            httpClient = webApplicationFactory.CreateClient(
                new WebApplicationFactoryClientOptions()
                {
                    HandleCookies = true,
                    AllowAutoRedirect = true
                });

            testScaffold.TestScaffoldContext.Set(httpClient, key);
        }

        return httpClient!;
    }

    private static string CreateHttpClientKey<TWebApplicationFactory, TEntry>()
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        return $"HttpClient__{typeof(TWebApplicationFactory).FullName}";
    }
}
