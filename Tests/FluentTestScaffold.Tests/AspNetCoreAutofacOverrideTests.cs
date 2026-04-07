using Autofac;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using AutofacApp = FluentTestScaffold.Sample.AutofacWebApp;
using WebApp = FluentTestScaffold.Sample.WebApp;

namespace FluentTestScaffold.Tests;

/// <summary>
/// Tests verifying configureHost and configureServices behavior against real app entry points.
/// Uses two separate sample apps: one with Autofac DI and one with default .NET IOC.
/// </summary>
[TestFixture]
public class AspNetCoreAutofacOverrideTests
{
    #region Autofac app tests (AutofacProgram)

    [Test]
    public void AutofacApp_ConfigureServices_IsOverwrittenByAutofacModules()
    {
        // Arrange — attempt to override IOverrideTestService (registered by the app's
        // Autofac module in Program.cs) using configureServices
        var mockService = new MockAutofacOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<AutofacApp.AutofacProgram>(
                configureServices: services =>
                    services.AddSingleton<AutofacApp.IOverrideTestService>(mockService));

        // Assert — the app's Autofac module re-registers IOverrideTestService after
        // ConfigureTestServices, so the configureServices override is lost
        var resolved = testScaffold.Resolve<AutofacApp.IOverrideTestService>();
        Assert.That(resolved, Is.Not.SameAs(mockService));
        Assert.That(resolved, Is.InstanceOf<AutofacApp.RealOverrideService>());
    }

    [Test]
    public void AutofacApp_ConfigureHost_OverridesAutofacModuleRegistrations()
    {
        // Arrange — use configureHost to override IOverrideTestService via ConfigureContainer,
        // which runs after the app's Autofac module
        var mockService = new MockAutofacOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<AutofacApp.AutofacProgram>(
                configureHost: hostBuilder =>
                    hostBuilder.ConfigureContainer<ContainerBuilder>(cb =>
                        cb.RegisterInstance(mockService).As<AutofacApp.IOverrideTestService>()));

        // Assert — configureHost runs after app-level ConfigureContainer, so the mock wins
        var resolved = testScaffold.Resolve<AutofacApp.IOverrideTestService>();
        Assert.That(resolved, Is.SameAs(mockService));
    }

    [Test]
    public void AutofacApp_ConfigureHost_NonOverriddenServiceStillResolves()
    {
        // Arrange — override IOverrideTestService but leave IOtherTestService untouched
        var mockService = new MockAutofacOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<AutofacApp.AutofacProgram>(
                configureHost: hostBuilder =>
                    hostBuilder.ConfigureContainer<ContainerBuilder>(cb =>
                        cb.RegisterInstance(mockService).As<AutofacApp.IOverrideTestService>()));

        // Assert — the overridden service is the mock
        var overridden = testScaffold.Resolve<AutofacApp.IOverrideTestService>();
        Assert.That(overridden, Is.SameAs(mockService));

        // Assert — the non-overridden service still resolves to the app's real implementation
        var other = testScaffold.Resolve<AutofacApp.IOtherTestService>();
        Assert.That(other, Is.InstanceOf<AutofacApp.RealOtherService>());
    }

    #endregion

    #region Default .NET IOC app tests (Program)

    [Test]
    public void DefaultIoc_ConfigureServices_OverridesAppRegistration()
    {
        // Arrange — override IOverrideTestService registered in Program.cs via configureServices
        var mockService = new MockWebAppOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>(
                configureServices: services =>
                    services.AddSingleton<WebApp.IOverrideTestService>(mockService));

        // Assert — with default .NET IOC, configureServices (ConfigureTestServices) works
        // because there is no ConfigureContainer to overwrite it
        var resolved = testScaffold.Resolve<WebApp.IOverrideTestService>();
        Assert.That(resolved, Is.SameAs(mockService));
    }

    [Test]
    public void DefaultIoc_ConfigureHost_OverridesAppRegistration()
    {
        // Arrange — override IOverrideTestService using configureHost with ConfigureServices
        var mockService = new MockWebAppOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>(
                configureHost: hostBuilder =>
                    hostBuilder.ConfigureServices(services =>
                        services.AddSingleton<WebApp.IOverrideTestService>(mockService)));

        // Assert — configureHost with ConfigureServices also works for default IOC
        var resolved = testScaffold.Resolve<WebApp.IOverrideTestService>();
        Assert.That(resolved, Is.SameAs(mockService));
    }

    [Test]
    public void DefaultIoc_ConfigureServices_NonOverriddenServiceStillResolves()
    {
        // Arrange — override IOverrideTestService but leave IOtherTestService untouched
        var mockService = new MockWebAppOverrideService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>(
                configureServices: services =>
                    services.AddSingleton<WebApp.IOverrideTestService>(mockService));

        // Assert — the overridden service is the mock
        var overridden = testScaffold.Resolve<WebApp.IOverrideTestService>();
        Assert.That(overridden, Is.SameAs(mockService));

        // Assert — the non-overridden service still resolves to the app's real implementation
        var other = testScaffold.Resolve<WebApp.IOtherTestService>();
        Assert.That(other, Is.InstanceOf<WebApp.RealOtherService>());
    }

    #endregion
}

#region Test Mocks

public class MockAutofacOverrideService : AutofacApp.IOverrideTestService { }

public class MockWebAppOverrideService : WebApp.IOverrideTestService { }

#endregion
