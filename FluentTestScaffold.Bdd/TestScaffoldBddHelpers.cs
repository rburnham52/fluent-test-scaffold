using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core;

/// <summary>
/// Shared helper methods used by both synchronous and asynchronous BDD extension classes
/// </summary>
internal static class TestScaffoldBddHelpers
{
    internal static TestScaffold PerformAction(TestScaffold testScaffold, string actionType, string description, Action<TestScaffold> action)
    {
        LogStep(testScaffold, actionType, description);
        action(testScaffold);
        return testScaffold;
    }

    internal static TestScaffold PerformAction<TService>(TestScaffold testScaffold, string actionType, string description, Action<TService> action) where TService : notnull
    {
        LogStep(testScaffold, actionType, description);
        var service = testScaffold.Resolve<TService>();
        action(service);
        return testScaffold;
    }

    internal static async Task<TestScaffold> PerformActionAsync(TestScaffold testScaffold, string actionType, string description, Func<TestScaffold, Task> action)
    {
        LogStep(testScaffold, actionType, description);
        await action(testScaffold);
        return testScaffold;
    }

    internal static async Task<TestScaffold> PerformActionAsync<TService>(TestScaffold testScaffold, string actionType, string description, Func<TService, Task> action) where TService : notnull
    {
        LogStep(testScaffold, actionType, description);
        var service = testScaffold.Resolve<TService>();
        await action(service);
        return testScaffold;
    }

    internal static void LogStep(TestScaffold testScaffold, string actionType, string description)
    {
        var logger = testScaffold.ServiceProvider?.GetService<ITestScaffoldLogger>();
        logger?.Info(actionType + " " + description);
    }
}
