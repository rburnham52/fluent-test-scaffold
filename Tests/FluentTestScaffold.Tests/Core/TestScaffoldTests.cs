using System;
using System.Collections.Generic;
using System.Reflection;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.CustomBuilder.DataTemplates;
using FluentTestScaffold.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class TestScaffoldTests
{
    [Test]
    public void TestScaffold_UsingBuilder_Resolves_Registered_Builder()
    {
        var testScaffold = new TestScaffold()
            .UseIoc();

        var builder = testScaffold.UsingBuilder<MockBuilder>();
        Assert.IsNotNull(builder);
    }

    [Test]
    public void TestScaffold_Resolve_Service()
    {
        var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddTransient<MockService>());

        var service = testScaffold.Resolve<MockService>();
        Assert.IsNotNull(service);
    }


    [Test]
    public void TestScaffold_Resolve_With_Fluent_Api()
    {
        MockService? service = null;
        new TestScaffold()
            .UseIoc(ctx => { ctx.Container.AddTransient<MockService>(); })
            .Resolve<MockService>(out service)
            .UsingBuilder<MockBuilder>();

        Assert.IsNotNull(service);
    }

    [Test]
    public void TestScaffold_UseIoc_With_Null_Factory_Throws_Exception()
    {
        DotnetServiceBuilder serviceBuilder = null!;
        Assert.Catch<ArgumentNullException>(() =>
            new TestScaffold().UseIoc(serviceBuilder!));
    }


    [Test]
    public void TestScaffold_Resolve_Without_Ioc_Built()
    {
        var testScaffold = new TestScaffold().Resolve<TestScaffold>();
        Assert.IsNotNull(testScaffold);
    }

    [Test]
    public void TestScaffold_RegisterBuilders_Auto_Build_Ioc()
    {
        var mockBuilder = new TestScaffold()
            .UseIoc()
            .Resolve<MockBuilder>();
        Assert.IsTrue(mockBuilder.GetType() == typeof(MockBuilder));
    }

    [Test]
    public void TestScaffold_Can_Apply_Templates_By_MethodName()
    {
        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.DataTemplates,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc()
            .WithTemplate<TestScaffoldDataTemplates>(dt => dt.SetContextFromTemplate());

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplate", out bool applied);

        Assert.IsTrue(applied);
    }

    [Test]
    public void TestScaffold_Can_Apply_Templates_By_AttributeName()
    {
        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.All,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc()
            .WithTemplate<TestScaffoldDataTemplates>(dt => dt.TemplateMatchedByAttributeName());

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplateAttributeName", out bool applied);

        Assert.IsTrue(applied);
    }


    [Test]
    public void TestScaffold_Can_Apply_Pass_Parameters_To_Data_Template()
    {
        var id = Guid.NewGuid();

        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.All,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc()
            .WithTemplate<TestScaffoldDataTemplates>(dt => dt.SetContextFromTemplateParameter(id));

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplateParameter", out Guid idFromContext);

        Assert.AreEqual(id, idFromContext);
    }

    [Test]
    public void TestScaffold_DataTemplates_Constructor_Injection_Works()
    {
        // Arrange: Create a test scaffold with custom services
        var testValue = Guid.NewGuid();
        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.DataTemplates,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc(ctx =>
            {
                // Register services that the template constructor actually needs
                ctx.Container.AddSingleton<MockService>();
                ctx.Container.AddSingleton<object>(testValue);
            });

        // Act: Apply the template that actually uses injected services
        testScaffold.WithTemplate<TestScaffoldDataTemplates>(dt => dt.SetContextFromTemplateWithInjectedServices());

        // Assert: Verify the template was executed with injected services
        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplateWithInjectedServices", out bool applied);
        Assert.IsTrue(applied, "Template should have been executed, indicating constructor injection worked");

        // Verify the injected services were actually used
        testScaffold.TestScaffoldContext.TryGetValue("InjectedServiceType", out string? serviceType);
        Assert.AreEqual("MockService", serviceType, "Template should have used the injected MockService");

        testScaffold.TestScaffoldContext.TryGetValue("InjectedTestValue", out object? injectedValue);
        Assert.AreEqual(testValue, injectedValue, "Template should have used the injected test value");

        // Additional verification: Try to resolve the template directly to ensure it's properly registered
        var resolvedTemplate = testScaffold.Resolve<TestScaffoldDataTemplates>();
        Assert.IsNotNull(resolvedTemplate, "Template should be resolvable from IOC container");
    }

    [Test]
    public void TestScaffold_DataTemplates_Throws_When_Required_Service_Not_Registered()
    {
        // Arrange: Create a test scaffold with DataTemplates auto-discovery but missing required services
        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.DataTemplates,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc(ctx =>
            {
                // Only register TestScaffold (which is always available)
                // Don't register MockService or object that the template constructor needs
            });

        // Act & Assert: Should throw when trying to resolve the template due to missing dependencies
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            testScaffold.WithTemplate<TestScaffoldDataTemplates>(dt => dt.SetContextFromTemplateWithInjectedServices());
        });

        // Verify the exception message indicates the issue
        Assert.IsTrue(exception.Message.Contains("MockService") ||
                     exception.Message.Contains("object") ||
                     exception.Message.Contains("Unable to resolve") ||
                     exception.Message.Contains("No service for type"),
                     "Exception should indicate which service couldn't be resolved");
    }

    [Test]
    public void TestScaffold_Template_With_Multiple_Param_Types()
    {
        var param1 = 32;
        var param2 = Guid.NewGuid();
        var param3 = "Hello World";

        var testScaffold = new TestScaffold(new ConfigOptions
        {
            AutoDiscovery = AutoDiscovery.All,
            Assemblies = new List<Assembly> { typeof(TestScaffoldDataTemplates).Assembly }
        })
            .UseIoc()
            .WithTemplate<TestScaffoldDataTemplates>(dt => dt.SetContextFromTemplateMultipleParameters(param1, param2, param3));

        testScaffold.TestScaffoldContext.TryGetValue("param1", out int actualParam1);
        testScaffold.TestScaffoldContext.TryGetValue("param2", out Guid actualParam2);
        testScaffold.TestScaffoldContext.TryGetValue("param3", out string? actualParam3);

        Assert.AreEqual(param1, actualParam1);
        Assert.AreEqual(param2, actualParam2);
        Assert.AreEqual(param3, actualParam3);
    }
}