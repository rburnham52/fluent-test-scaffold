using System.Collections.Generic;
using System.Reflection;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Tests.CustomBuilder.DefaultIoc;

public class DotnetIocAppServicesBuilder : DotnetServiceBuilder<DotnetIocAppServicesBuilder>
{
    private static readonly ConfigOptions Options = new()
    {
        AutoDiscovery = AutoDiscovery.All,
        Assemblies = new List<Assembly>() { Assembly.GetCallingAssembly() }
    };

    public DotnetIocAppServicesBuilder() : base(Options) { }
    public DotnetIocAppServicesBuilder(ConfigOptions configOptions) : base(configOptions) { }

    public void RegisterAppServices()
    {
        Container.AddSingleton(_ => TestDbContextFactory.Create());
        Container.AddTransient<IUserRequestContext, UserRequestContext>();
        Container.AddTransient<IAuthService, AuthService>();
        Container.AddTransient<ShoppingCart>();
    }

}