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
                Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
            })
            .UseIoc()
            .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplate));

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplate", out bool applied);

        Assert.IsTrue(applied);
    }

    [Test]
    public void TestScaffold_Can_Apply_Templates_By_AttributeName()
    {
        var testScaffold = new TestScaffold(new ConfigOptions
            {
                AutoDiscovery = AutoDiscovery.All,
                Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
            })
            .UseIoc()
            .WithTemplate(TestScaffoldDataTemplates.TemplateAttributeName);

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplate", out bool applied);

        Assert.IsTrue(applied);
    }


    [Test]
    public void TestScaffold_Can_Apply_Pass_Parameters_To_Data_Template()
    {
        var id = Guid.NewGuid();

        var testScaffold = new TestScaffold(new ConfigOptions
            {
                AutoDiscovery = AutoDiscovery.All,
                Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
            })
            .UseIoc()
            .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateParameter), id);

        testScaffold.TestScaffoldContext.TryGetValue("AppliedByTemplateParameter", out Guid idFromContext);

        Assert.AreEqual(id, idFromContext);
    }


    [Test]
    public void TestScaffold_Template_With_Mismatched_Parameters_Throws_Exception()
    {
        var id = Guid.NewGuid();

        var exception = Assert.Throws<DataTemplateException>(() =>
        {
            new TestScaffold(new ConfigOptions
                {
                    AutoDiscovery = AutoDiscovery.All,
                    Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
                })
                .UseIoc()
                .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplate), id);
        });
        Assert.AreEqual("Failed to apply template 'SetContextFromTemplate'. Parameter count mismatch.'", exception?.Message);
    }


    [Test]
    public void TestScaffold_Template_With_Incorrect_Parameter_Type()
    {
        var incorrectParamType = 32;

        var exception = Assert.Throws<DataTemplateException>(() =>
        {
            new TestScaffold(new ConfigOptions
                {
                    AutoDiscovery = AutoDiscovery.All,
                    Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
                })
                .UseIoc()
                .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateParameter), incorrectParamType);
        });
        Assert.AreEqual(
            "Failed to apply template 'SetContextFromTemplateParameter'. Object of type 'System.Int32' cannot be converted to type 'System.Guid'.'",
            exception?.Message);
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
                Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
            })
            .UseIoc()
            .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateMultipleParameters), param1, param2,
                param3);

        testScaffold.TestScaffoldContext.TryGetValue("param1", out int actualParam1);
        testScaffold.TestScaffoldContext.TryGetValue("param2", out Guid actualParam2);
        testScaffold.TestScaffoldContext.TryGetValue("param3", out string? actualParam3);

        Assert.AreEqual(param1, actualParam1);
        Assert.AreEqual(param2, actualParam2);
        Assert.AreEqual(param3, actualParam3);
    }

    [Test]
    public void TestScaffold_Template_With_Multiple_Param_Missing_A_Parameter()
    {
        var param1 = 32;
        var param2 = Guid.NewGuid();

        var exception = Assert.Throws<DataTemplateException>(() =>
        {
            new TestScaffold(new ConfigOptions
                {
                    AutoDiscovery = AutoDiscovery.All,
                    Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
                })
                .UseIoc()
                .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateMultipleParameters), param1,
                    param2);
        });
        Assert.AreEqual("Failed to apply template 'SetContextFromTemplateMultipleParameters'. Parameter count mismatch.'",
            exception?.Message);
    }

    [Test]
    public void TestScaffold_Template_With_Multiple_Params_TypeMismatch()
    {
        var param1 = 32;
        var param2 = Guid.NewGuid();
        var param3 = "Hello World";
        var exception = Assert.Throws<DataTemplateException>(() =>
        {
            new TestScaffold(new ConfigOptions
                {
                    AutoDiscovery = AutoDiscovery.All,
                    Assemblies = new List<Assembly> {typeof(TestScaffoldDataTemplates).Assembly}
                })
                .UseIoc()
                .WithTemplate(nameof(TestScaffoldDataTemplates.SetContextFromTemplateMultipleParameters), param3,
                    param2, param1);
        });
        Assert.AreEqual("Failed to apply template 'SetContextFromTemplateMultipleParameters'. Object of type 'System.String' cannot be converted to type 'System.Int32'.'",
            exception?.Message);
    }

}