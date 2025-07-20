using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Core;

public static class TestScaffoldServiceRegistration
{
    public static void AddTestScaffoldServices(this IServiceCollection services, TestScaffold testScaffold)
    {
        services.AddSingleton(testScaffold);
        services.AddSingleton(testScaffold.TestScaffoldContext);
        services.AddSingleton(DefaultLogger.Logger);
        services.AddSingleton(testScaffold.Options);
        
        services.RegisterBuildersWithAutoDiscovery(testScaffold.Options);
        services.RegisterDataTemplatesWithAutoDiscovery(testScaffold.Options);
    }
    
    public static void RegisterBuildersWithAutoDiscovery(this IServiceCollection services, ConfigOptions configOptions)
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
    
    public static void RegisterDataTemplatesWithAutoDiscovery(this IServiceCollection services, ConfigOptions configOptions)
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
