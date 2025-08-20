using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;

namespace FluentTestScaffold.Tests.CustomBuilder.Autofac;

public class AutofacAppServicesBuilder : AutofacServiceBuilder<AutofacAppServicesBuilder>
{
    private static readonly ConfigOptions ConfigOptions = new()
    {
        // Auto Discovery any Builders and DataTemplates in the calling assembly
        AutoDiscovery = AutoDiscovery.All,
        Assemblies = new List<Assembly> { Assembly.GetCallingAssembly() }
    };

    public AutofacAppServicesBuilder() : base(ConfigOptions) { }

    public AutofacAppServicesBuilder RegisterAppServices(Func<UserRequestContext, User?>? authenticateUser = null)
    {
        Container.Register<TestDbContext>(_ => TestDbContextFactory.Create()).SingleInstance();
        Container.Register(c =>
            {
                var requestContext = new UserRequestContext(c.Resolve<IAuthService>());
                if (authenticateUser != null)
                    authenticateUser(requestContext);
                return requestContext;
            })
            .As<IUserRequestContext>()
            .InstancePerLifetimeScope();
        Container.RegisterType<AuthService>().As<IAuthService>();
        Container.RegisterType<ShoppingCart>();

        return this;
    }

    public AutofacAppServicesBuilder AutoDiscover()
    {
        RegisterBuildersWithAutoDiscovery();
        RegisterDataTemplatesWithAutoDiscovery();
        return this;
    }
}
