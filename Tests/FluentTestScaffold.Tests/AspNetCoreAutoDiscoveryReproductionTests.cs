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
            .UseIoc();

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

        Assert.Throws<InvalidOperationException>(() => testScaffold.Resolve<InventoryBuilder>());
        
        var dataTemplateService = testScaffold.Resolve<DataTemplateService>();
        Assert.Throws<MissingMethodException>(() => dataTemplateService.FindByName(TestScaffoldDataTemplates.TemplateAttributeName));
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
