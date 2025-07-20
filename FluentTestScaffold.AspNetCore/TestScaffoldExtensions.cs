using System.Reflection;
using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.AspNetCore;

public static class TestScaffoldExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="configureServices">Optional configuration for additional services</param>
    public static TestScaffold UseAspNet<TEntryPoint>(
        this TestScaffold testScaffold,
        Action<IServiceCollection>? configureServices = null)
        where TEntryPoint : class
    {
        var factory = new AspNetWebApplicationFactory<TEntryPoint>(testScaffold.Options, configureServices);
        testScaffold.TestScaffoldContext.Set(factory);
        return testScaffold.WithServiceProvider(factory.Services);
    }

    /// <summary>
    /// </summary>
    /// <param name="configureServices">Optional configuration for additional services</param>
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
                services.AddSingleton(testScaffold.Options);
                services.AddSingleton(testScaffold.TestScaffoldContext);
                services.AddSingleton(DefaultLogger.Logger);

                RegisterBuildersWithAutoDiscovery(services, testScaffold.Options);
                RegisterDataTemplatesWithAutoDiscovery(services, testScaffold.Options);

                configureServices?.Invoke(services);
            });
        });

        testScaffold.TestScaffoldContext.Set(enhancedFactory);
        return testScaffold.WithServiceProvider(enhancedFactory.Services);
    }

    /// <summary>
    /// </summary>
    [Obsolete("Use UseAspNet<TEntryPoint>() instead. This method will be removed in a future version.")]
    public static TestScaffold WithWebApplicationFactory<TWebApplicationFactory, TEntry>(
        this TestScaffold testScaffold,
        TWebApplicationFactory webApplicationFactory)
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        testScaffold.TestScaffoldContext.Set(webApplicationFactory);
        return testScaffold.WithServiceProvider(webApplicationFactory.Services);
    }

    public static HttpClient GetWebApplicationHttpClient<TWebApplicationFactory, TEntry>(this TestScaffold testScaffold)
        where TWebApplicationFactory : WebApplicationFactory<TEntry>
        where TEntry : class
    {
        if (!testScaffold.TestScaffoldContext.TryGetValue<TWebApplicationFactory>(out var webApplicationFactory))
        {
            throw new InvalidOperationException(
                "A call to testScaffold.WithWebApplicationFactory<TFactory, TEntryPoint>(factory) is required" +
                " to initialise the a web application factory before a HttpClient can be created");
        }

        var key = CreateHttpClientKey<TWebApplicationFactory, TEntry>();

        if (!testScaffold.TestScaffoldContext.TryGetValue<HttpClient>(key, out var httpClient))
        {
            httpClient = webApplicationFactory!.CreateClient(
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

    private static void RegisterBuildersWithAutoDiscovery(IServiceCollection services, ConfigOptions configOptions)
    {
        if (!configOptions.AutoDiscovery.HasFlag(AutoDiscovery.Builders)) return;
        foreach (var assembly in configOptions.Assemblies)
        {
            var builderTypes = assembly.GetTypes()
                .Where(t => t.IsAssignableToGenericType(typeof(Builder<>)))
                .Where(t => t != typeof(Builder<>))
                .ToArray();
            foreach (var builderType in builderTypes)
            {
                services.AddSingleton(builderType);
            }
        }
    }

    private static void RegisterDataTemplatesWithAutoDiscovery(IServiceCollection services, ConfigOptions configOptions)
    {
        var dataTemplateMethods = new List<MethodInfo>();
        if (configOptions.AutoDiscovery.HasFlag(AutoDiscovery.DataTemplates))
        {
            foreach (var assembly in configOptions.Assemblies)
            {
                var methods = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.GetCustomAttributes(typeof(DataTemplateAttribute), false).Length >= 1)
                    .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(TestScaffold))
                    .ToArray();
                dataTemplateMethods.AddRange(methods);
            }
        }
        var dataTemplateService = new DataTemplateService(dataTemplateMethods);
        services.AddSingleton(dataTemplateService);
    }
}
