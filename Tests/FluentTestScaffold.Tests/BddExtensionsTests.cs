using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class BddExtensionsTests
{
    private ITestScaffoldLogger _originalLogger = null!;
    private RecordingLogger _logger = null!;

    [SetUp]
    public void Setup()
    {
        _originalLogger = DefaultLogger.Logger;
        _logger = new RecordingLogger();
        DefaultLogger.Logger = _logger;
    }

    [TearDown]
    public void TearDown()
    {
        DefaultLogger.Logger = _originalLogger;
    }

    private static TestScaffold CreateTestScaffold()
    {
        return new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton<MockService>());
    }

    [Test]
    public void Steps_With_TestScaffold_Execute_In_Order_And_Log_Each_Step()
    {
        var executionOrder = new List<string>();
        using var testScaffold = CreateTestScaffold();

        var result = testScaffold
            .Scenario("a scenario")
            .Given("a precondition", _ => executionOrder.Add("given"))
            .And("another precondition", _ => executionOrder.Add("and"))
            .When("an action", _ => executionOrder.Add("when"))
            .Then("an outcome", _ => executionOrder.Add("then"));

        Assert.AreSame(testScaffold, result);
        CollectionAssert.AreEqual(new[] { "given", "and", "when", "then" }, executionOrder);
        CollectionAssert.AreEqual(new[]
        {
            "Scenario a scenario",
            "Given a precondition",
            "And another precondition",
            "When an action",
            "Then an outcome"
        }, _logger.Messages);
    }

    [Test]
    public void Steps_With_Injected_Service_Resolve_The_Service_And_Log_Each_Step()
    {
        using var testScaffold = CreateTestScaffold();

        var result = testScaffold
            .Given<MockService>("a precondition", service => service.Increment())
            .And<MockService>("another precondition", service => service.Increment())
            .When<MockService>("an action", service => service.Increment())
            .Then<MockService>("an outcome", service => service.Increment());

        Assert.AreSame(testScaffold, result);
        Assert.AreEqual(4, testScaffold.Resolve<MockService>().Count);
        CollectionAssert.AreEqual(new[]
        {
            "Given a precondition",
            "And another precondition",
            "When an action",
            "Then an outcome"
        }, _logger.Messages);
    }

    [Test]
    public void Scenario_Without_Ioc_Built_Does_Not_Throw()
    {
        using var testScaffold = new TestScaffold();

        Assert.DoesNotThrow(() => testScaffold.Scenario("a scenario"));
        Assert.IsNull(testScaffold.ServiceProvider);
        Assert.IsEmpty(_logger.Messages);
    }

    [Test]
    public void Step_Exception_Is_Not_Swallowed()
    {
        using var testScaffold = CreateTestScaffold();

        Assert.Throws<ArgumentException>(() =>
            testScaffold.When("an action fails", _ => throw new ArgumentException("failed")));
    }

    [Test]
    public void Catch_Rethrows_Unexpected_Exception_Type()
    {
        using var testScaffold = CreateTestScaffold();

        Assert.Throws<InvalidOperationException>(() =>
            testScaffold.Catch<ArgumentException>(() => throw new InvalidOperationException("unexpected")));

        Assert.IsFalse(testScaffold.TestScaffoldContext.ContainsKey("CaughtException"));
    }

    [Test]
    public void Catch_Stores_Derived_Exception_Type()
    {
        using var testScaffold = CreateTestScaffold();

        testScaffold.Catch<ArgumentException>(() => throw new ArgumentNullException("param"));

        ArgumentException? handled = null;
        testScaffold.Handle<ArgumentException>(ex => handled = ex);

        Assert.IsInstanceOf<ArgumentNullException>(handled);
    }

    [Test]
    public void Catch_When_No_Exception_Thrown_Stores_Nothing()
    {
        using var testScaffold = CreateTestScaffold();

        testScaffold.Catch<InvalidOperationException>(() => { });

        Assert.IsFalse(testScaffold.TestScaffoldContext.ContainsKey("CaughtException"));
    }

    [Test]
    public void Handle_Without_Caught_Exception_Throws()
    {
        using var testScaffold = CreateTestScaffold();
        var handlerCalled = false;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            testScaffold.Handle<ArgumentException>(_ => handlerCalled = true));

        Assert.IsFalse(handlerCalled);
        Assert.That(exception!.Message, Does.Contain("Handle<ArgumentException>"));
        Assert.That(exception.Message, Does.Contain("Catch<ArgumentException>"));
    }

    [Test]
    public void HandleAsync_Without_Caught_Exception_Throws()
    {
        using var testScaffold = CreateTestScaffold();
        var handlerCalled = false;

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await testScaffold.HandleAsync<ArgumentException>(_ =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            }));

        Assert.IsFalse(handlerCalled);
        Assert.That(exception!.Message, Does.Contain("Catch<ArgumentException>"));
    }

    [Test]
    public async Task CatchAsync_When_No_Exception_Thrown_Stores_Nothing()
    {
        using var testScaffold = CreateTestScaffold();

        await testScaffold.CatchAsync<InvalidOperationException>(() => Task.CompletedTask);

        Assert.IsFalse(testScaffold.TestScaffoldContext.ContainsKey("CaughtException"));
    }

    [Test]
    public async Task Async_Steps_Starting_From_TestScaffold_Log_Each_Step()
    {
        using var testScaffold = CreateTestScaffold();

        await testScaffold.GivenAsync("a precondition", _ => Task.CompletedTask);
        await testScaffold.AndAsync("another precondition", _ => Task.CompletedTask);
        await testScaffold.AndAsync<MockService>("a service precondition", service =>
        {
            service.Increment();
            return Task.CompletedTask;
        });
        await testScaffold.WhenAsync("an action", _ => Task.CompletedTask);
        await testScaffold.WhenAsync<MockService>("a service action", service =>
        {
            service.Increment();
            return Task.CompletedTask;
        });
        await testScaffold.ThenAsync("an outcome", _ => Task.CompletedTask);
        await testScaffold.ThenAsync<MockService>("a service outcome", service =>
        {
            service.Increment();
            return Task.CompletedTask;
        });

        Assert.AreEqual(3, testScaffold.Resolve<MockService>().Count);
        CollectionAssert.AreEqual(new[]
        {
            "Given a precondition",
            "And another precondition",
            "And a service precondition",
            "When an action",
            "When a service action",
            "Then an outcome",
            "Then a service outcome"
        }, _logger.Messages);
    }

    [Test]
    public async Task Sync_Steps_Can_Be_Used_In_An_Async_Chain()
    {
        var executionOrder = new List<string>();
        using var testScaffold = CreateTestScaffold();

        var result = await testScaffold
            .GivenAsync("an async precondition", _ => Task.CompletedTask)
            .ScenarioAsync("a scenario")
            .GivenAsync("a precondition", (Action<TestScaffold>)(_ => executionOrder.Add("given")))
            .GivenAsync<MockService>("a service precondition", (Action<MockService>)(service => service.Increment()))
            .AndAsync("another precondition", (Action<TestScaffold>)(_ => executionOrder.Add("and")))
            .AndAsync<MockService>("another service precondition", (Action<MockService>)(service => service.Increment()))
            .WhenAsync("an action", (Action<TestScaffold>)(_ => executionOrder.Add("when")))
            .WhenAsync<MockService>("a service action", (Action<MockService>)(service => service.Increment()))
            .ThenAsync("an outcome", (Action<TestScaffold>)(_ => executionOrder.Add("then")))
            .ThenAsync<MockService>("a service outcome", (Action<MockService>)(service => service.Increment()));

        Assert.AreSame(testScaffold, result);
        CollectionAssert.AreEqual(new[] { "given", "and", "when", "then" }, executionOrder);
        Assert.AreEqual(4, testScaffold.Resolve<MockService>().Count);
        CollectionAssert.AreEqual(new[]
        {
            "Given an async precondition",
            "Scenario a scenario",
            "Given a precondition",
            "Given a service precondition",
            "And another precondition",
            "And another service precondition",
            "When an action",
            "When a service action",
            "Then an outcome",
            "Then a service outcome"
        }, _logger.Messages);
    }

    [Test]
    public async Task Async_Steps_With_Injected_Service_Can_Be_Chained()
    {
        using var testScaffold = CreateTestScaffold();

        await testScaffold
            .GivenAsync("an async precondition", _ => Task.CompletedTask)
            .GivenAsync<MockService>("a service precondition", service =>
            {
                service.Increment();
                return Task.CompletedTask;
            })
            .WhenAsync<MockService>("a service action", service =>
            {
                service.Increment();
                return Task.CompletedTask;
            });

        Assert.AreEqual(2, testScaffold.Resolve<MockService>().Count);
    }

    private class RecordingLogger : ITestScaffoldLogger
    {
        public List<string> Messages { get; } = new();

        public void Info(string message) => Messages.Add(message);
    }
}
