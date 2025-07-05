using System;
using System.Collections.Generic;
using System.Linq;
using FluentTestScaffold.Core;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class BuilderTests
{
    [Test]
    public void Enqueue_Actions_Are_Applied_in_Order()
    {
        var builder = new TestScaffold()
            .UseIoc()
            .Resolve<MockBuilder>();

        builder.Enqueue("first");
        builder.Enqueue("second");
        builder.Enqueue("third");

        Assert.AreEqual(0, builder.AppliedOrder.Count, "No action should be applied before build is called");
        builder.Build();
        var applied = string.Join(",", builder.AppliedOrder);
        Assert.AreEqual("first,second,third", applied, "Applied actions do not match the expected order");
    }

    [Test]
    public void ServiceProvider_Can_Resolve_From_Internal_Ioc()
    {
        var builder = new TestScaffold()
            .UseIoc();

        var resolvedScaffold = builder.ServiceProvider!.GetService<TestScaffold>();
        Assert.AreEqual(builder, resolvedScaffold);
    }

    [Test]
    public void Builder_UsingTestScaffold_Can_Switch_Context()
    {
        var testScaffold = new TestScaffold()
            .UseIoc()
            .UsingBuilder<MockBuilder>()
            .UsingTestScaffold();

        Assert.IsTrue(testScaffold.GetType() == typeof(TestScaffold));
    }


    [Test]
    public void Builder_TestContext_Can_Add_To_TestContext()
    {
        var context = new TestScaffold()
            .UseIoc()
            .UsingBuilder<MockBuilder>()
            //Enqueue saves FirstAdded & LastAdded to TestContext with 2 different methods
            .Enqueue("5")
            .Enqueue("6")
            .Enqueue("3")
            .UsingTestScaffold()
            .TestScaffoldContext;


        context.TryGetValue("FirstAdded", out var firstAdded);
        context.TryGetValue("LastAdded", out var lastAdded);


        Assert.IsNotNull(firstAdded);
        Assert.IsNotNull(lastAdded);
        Assert.AreEqual("5", firstAdded!);
        Assert.AreEqual("3", lastAdded!);
    }
    
    [Test]
    public void Builder_If_Conditionally_Applies_Action()
    {
       var testScaffold = new TestScaffold()
            .UseIoc()
            .UsingBuilder<MockBuilder>()
            .If(false, b => b.Enqueue("5"))
            .Enqueue("6")
            .If(true, b => b.Enqueue("1"))
            .Enqueue("3")
            .Build();

       var builder = testScaffold
           .UsingBuilder<MockBuilder>();
       
        Assert.IsNotEmpty(builder.AppliedOrder);
        Assert.AreEqual(builder.AppliedOrder.Contains("5"), false);
        Assert.AreEqual(builder.AppliedOrder.Contains("6"), true);
        Assert.AreEqual(builder.AppliedOrder.Contains("1"), true);
        Assert.AreEqual(builder.AppliedOrder.Contains("3"), true);
    }
    
    [Test]
    [TestCase(true, "was true")]
    [TestCase(false, "was false")]
    public void Builder_IfElse_Conditionally_Applies_Action_Or_Alternative(bool condition, string expected)
    {
        var testScaffold = new TestScaffold()
            .UseIoc()
            .UsingBuilder<MockBuilder>()
            .IfElse(condition, b => b.Enqueue("was true"), b => b.Enqueue("was false"))
            .Build();

        var builder = testScaffold
            .UsingBuilder<MockBuilder>();
       
        Assert.IsNotEmpty(builder.AppliedOrder);
        Assert.IsTrue(builder.AppliedOrder.Count == 1);
        Assert.AreEqual(builder.AppliedOrder.First(), expected);
    }


    private class MockBuilder : Builder<MockBuilder>
    {
        public MockBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public List<string> AppliedOrder { get; } = new();

        public new IServiceProvider ServiceProvider => base.ServiceProvider;

        internal MockBuilder Enqueue(string actionName)
        {
            base.Enqueue(_ => AppliedOrder.Add(actionName));
            var hasValue = TestScaffoldContext.ContainsKey("LastAdded");

            // Add to context Directly
            if (!hasValue)
                TestScaffoldContext["FirstAdded"] = actionName;

            // Add to context with fluent API
            SetTestContext("LastAdded", actionName);
            return this;
        }
    }
}