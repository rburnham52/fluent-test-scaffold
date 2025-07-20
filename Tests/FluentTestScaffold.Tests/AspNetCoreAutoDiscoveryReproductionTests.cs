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
    public void TestScaffold_UseIoc_AutoDiscovery_Works()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(c => c.RegisterSingleton(new TestService()));

        Assert.DoesNotThrow(() => testScaffold.Resolve<TestService>(), "TestService should be resolvable with UseIoc container");

        Assert.DoesNotThrow(() => testScaffold.Resolve<InventoryBuilder>());

        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.DoesNotThrow(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName));
    }

    [Test]
    public void TestScaffold_UseAspNet_AutoDiscovery_Works()
    {
        var testScaffold = new TestScaffold()
            .UseAspNet<Program>(services => services.AddSingleton<TestService>());

        Assert.DoesNotThrow(() => testScaffold.Resolve<TestService>(), "TestService should be resolvable with UseAspNet");

        Assert.DoesNotThrow(() => testScaffold.Resolve<InventoryBuilder>(), "InventoryBuilder should be resolvable with UseAspNet auto discovery");

        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.DoesNotThrow(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName), "DataTemplateService should be resolvable with UseAspNet auto discovery");
    }

    [Test]
    public void TestScaffold_UseAspNet_MutualExclusivity_ThrowsException()
    {
        var testScaffold = new TestScaffold()
            .UseIoc();

        Assert.Throws<InvalidOperationException>(() => testScaffold.UseAspNet<Program>(),
            "Should throw exception when trying to use UseAspNet after UseIoc");
    }

    [Test]
    public void TestScaffold_UseAspNet_WithCustomFactory_AutoDiscovery_Works()
    {
        var webApplicationFactory = new SampleWebApplicationFactory();

        var testScaffold = new TestScaffold()
            .UseAspNet<SampleWebApplicationFactory, Program>(webApplicationFactory);

        Assert.DoesNotThrow(() => testScaffold.Resolve<TestService>(), "TestService should be resolvable with UseAspNet custom factory");

        Assert.DoesNotThrow(() => testScaffold.Resolve<InventoryBuilder>(), "InventoryBuilder should be resolvable with UseAspNet custom factory auto discovery");

        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.DoesNotThrow(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName), "DataTemplateService should be resolvable with UseAspNet custom factory auto discovery");
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
