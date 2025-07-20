using System.Reflection;
using FluentTestScaffold.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.AspNetCore;

internal class AspNetWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly Action<IServiceCollection>? _configureServices;
    private readonly ConfigOptions _configOptions;

    public AspNetWebApplicationFactory(ConfigOptions configOptions, Action<IServiceCollection>? configureServices = null)
    {
        _configOptions = configOptions;
        _configureServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(_configOptions);
            services.AddSingleton<TestScaffoldContext>();
            services.AddSingleton(DefaultLogger.Logger);
            
            RegisterBuildersWithAutoDiscovery(services);
            RegisterDataTemplatesWithAutoDiscovery(services);
            
            _configureServices?.Invoke(services);
        });
    }

    private void RegisterBuildersWithAutoDiscovery(IServiceCollection services)
    {
        if (!_configOptions.AutoDiscovery.HasFlag(AutoDiscovery.Builders)) return;
        foreach (var assembly in _configOptions.Assemblies)
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

    private void RegisterDataTemplatesWithAutoDiscovery(IServiceCollection services)
    {
        var dataTemplateMethods = new List<MethodInfo>();
        if (_configOptions.AutoDiscovery.HasFlag(AutoDiscovery.DataTemplates))
        {
            foreach (var assembly in _configOptions.Assemblies)
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
