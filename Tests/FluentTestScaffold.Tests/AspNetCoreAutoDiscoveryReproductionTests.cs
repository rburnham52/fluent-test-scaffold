using System;
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
public class AspNetCoreAutoDiscoveryReproductionTests
{
    [Test]
    public void TestScaffold_WithoutWebApplicationFactory_AutoDiscovery_Works()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(c => c.RegisterSingleton(new TestService()));

        
        Assert.DoesNotThrow(() => testScaffold.Resolve<TestService>(), "TestService should be resolvable without WebApplicationFactory using Ioc container");
        
        Assert.DoesNotThrow(() => testScaffold.Resolve<InventoryBuilder>());
        
        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.DoesNotThrow(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName));
    }

    [Test]
    public void TestScaffold_WithWebApplicationFactory_AutoDiscovery_Fails()
    {
        var webApplicationFactory = new SampleWebApplicationFactory();
        
        var testScaffold = new TestScaffold()
            .WithWebApplicationFactory<SampleWebApplicationFactory, Program>(webApplicationFactory);

        Assert.DoesNotThrow(() => testScaffold.Resolve<TestService>(), "TestService should be resolvable with WebApplicationFactory only as it replaces the Ioc container");
        
        Assert.DoesNotThrow(() => testScaffold.Resolve<InventoryBuilder>(), "InventoryBuilder should be resolvable with default auto discovery settings");
        
        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.DoesNotThrow(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName), "DataTemplateService should be resolvable with default auto discovery settings");
    }
}

public class SampleWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TestService>();
        });
    }
}

public class TestService
{
    public string GetMessage() => "Test Service";
}
