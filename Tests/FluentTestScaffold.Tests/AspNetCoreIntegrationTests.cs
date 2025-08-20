using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.CustomBuilder;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class AspNetCoreIntegrationTests
{
    [Test]
    public void UseAspNet_WithSimpleEntryPoint_ShouldConfigureServices()
    {
        // Arrange & Act
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>();

        // Assert
        Assert.That(testScaffold.ServiceProvider, Is.Not.Null);
        Assert.DoesNotThrow(() => testScaffold.Resolve<TestScaffoldDataTemplates>());
    }

    [Test]
    public void UseAspNet_WithServiceConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var testService = new TestService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>(services => services.AddSingleton(testService));

        // Assert
        var resolvedService = testScaffold.Resolve<TestService>();
        Assert.That(resolvedService, Is.SameAs(testService));
    }

    [Test]
    public void UseAspNet_WithCustomFactory_ShouldUseCustomFactory()
    {
        // Arrange
        var customFactory = new CustomWebApplicationFactory();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<CustomWebApplicationFactory, Program>(customFactory);

        // Assert
        Assert.That(testScaffold.ServiceProvider, Is.Not.Null);
        Assert.DoesNotThrow(() => testScaffold.Resolve<TestScaffoldDataTemplates>());
    }

    [Test]
    public void UseAspNet_WithCustomFactoryAndConfiguration_ShouldApplyBoth()
    {
        // Arrange
        var customFactory = new CustomWebApplicationFactory();
        var testService = new TestService();

        // Act
        var testScaffold = new TestScaffold()
            .UseAspNet<CustomWebApplicationFactory, Program>(customFactory, services => services.AddSingleton(testService));

        // Assert
        var resolvedService = testScaffold.Resolve<TestService>();
        Assert.That(resolvedService, Is.SameAs(testService));
    }

    [Test]
    public void GetWebApplicationHttpClient_WithSimpleEntryPoint_ShouldReturnHttpClient()
    {
        // Arrange
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>();

        // Act
        var httpClient = testScaffold.GetWebApplicationHttpClient<Program>();

        // Assert
        Assert.That(httpClient, Is.Not.Null);
        Assert.That(httpClient.BaseAddress, Is.Not.Null);
    }

    [Test]
    public void GetWebApplicationHttpClient_WithCustomFactory_ShouldReturnHttpClient()
    {
        // Arrange
        var customFactory = new CustomWebApplicationFactory();
        var testScaffold = new TestScaffold()
            .UseAspNet<CustomWebApplicationFactory, Program>(customFactory);

        // Act
        var httpClient = testScaffold.GetWebApplicationHttpClient<CustomWebApplicationFactory, Program>();

        // Assert
        Assert.That(httpClient, Is.Not.Null);
        Assert.That(httpClient.BaseAddress, Is.Not.Null);
    }

    [Test]
    public void GetWebApplicationHttpClient_WithoutUseAspNet_ShouldThrowException()
    {
        // Arrange
        var testScaffold = new TestScaffold();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            testScaffold.GetWebApplicationHttpClient<Program>());

        Assert.That(exception.Message, Does.Contain("UseAspNet"));
    }

    [Test]
    public void GetWebApplicationHttpClient_WithCustomFactoryWithoutUseAspNet_ShouldThrowException()
    {
        // Arrange
        var testScaffold = new TestScaffold();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            testScaffold.GetWebApplicationHttpClient<CustomWebApplicationFactory, Program>());

        Assert.That(exception.Message, Does.Contain("UseAspNet"));
    }

    [Test]
    public void GetWebApplicationHttpClient_MultipleCalls_ShouldReturnSameInstance()
    {
        // Arrange
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>();

        // Act
        var httpClient1 = testScaffold.GetWebApplicationHttpClient<Program>();
        var httpClient2 = testScaffold.GetWebApplicationHttpClient<Program>();

        // Assert
        Assert.That(httpClient1, Is.SameAs(httpClient2));
    }

    [Test]
    public void GetWebApplicationHttpClient_WithCustomFactory_MultipleCalls_ShouldReturnSameInstance()
    {
        // Arrange
        var customFactory = new CustomWebApplicationFactory();
        var testScaffold = new TestScaffold()
            .UseAspNet<CustomWebApplicationFactory, Program>(customFactory);

        // Act
        var httpClient1 = testScaffold.GetWebApplicationHttpClient<CustomWebApplicationFactory, Program>();
        var httpClient2 = testScaffold.GetWebApplicationHttpClient<CustomWebApplicationFactory, Program>();

        // Assert
        Assert.That(httpClient1, Is.SameAs(httpClient2));
    }

    [Test]
    public void UseAspNet_WithNullServiceConfiguration_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var testScaffold = new TestScaffold()
                .UseAspNet<Program>(null);
        });
    }

    [Test]
    public void UseAspNet_WithCustomFactoryAndNullConfiguration_ShouldNotThrow()
    {
        // Arrange
        var customFactory = new CustomWebApplicationFactory();

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var testScaffold = new TestScaffold()
                .UseAspNet<CustomWebApplicationFactory, Program>(customFactory, null);
        });
    }

    [Test]
    public void AspNetWebApplicationFactory_Dispose_ShouldNotThrow()
    {
        // Arrange
        var testScaffold = new TestScaffold();
        var factory = new AspNetWebApplicationFactory<Program>(testScaffold);

        // Act & Assert
        Assert.DoesNotThrow(() => factory.Dispose());
    }

    [Test]
    public void AspNetWebApplicationFactory_WithServiceConfiguration_ShouldInvokeConfiguration()
    {
        // Arrange
        var testScaffold = new TestScaffold();
        var configurationCalled = false;
        var factory = new AspNetWebApplicationFactory<Program>(testScaffold, services =>
        {
            configurationCalled = true;
            services.AddSingleton(new TestService());
        });

        // Act
        var services = factory.Services;

        // Assert
        Assert.That(configurationCalled, Is.True);
        Assert.That(services.GetService<TestService>(), Is.Not.Null);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TestService>();
        });
    }
}
