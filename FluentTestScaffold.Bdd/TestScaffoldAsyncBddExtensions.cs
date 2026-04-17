// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core
{
    /// <summary>
    /// Extension methods to support asynchronous BDD style Tests with the Test Scaffold
    /// </summary>
    public static class TestScaffoldAsyncBddExtensions
    {
        // === Async extensions on TestScaffold ===

        /// <summary>
        /// Defines an asynchronous given step to prepare your test state
        /// </summary>
        public static Task<TestScaffold> GivenAsync(this TestScaffold testScaffold, string description, Func<TestScaffold, Task> action)
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous given step to prepare your test state and injects a specific service into the step definition
        /// </summary>
        public static Task<TestScaffold> GivenAsync<TService>(this TestScaffold testScaffold, string description, Func<TService, Task> action) where TService : notnull
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous when step to perform an action
        /// </summary>
        public static Task<TestScaffold> WhenAsync(this TestScaffold testScaffold, string description, Func<TestScaffold, Task> action)
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous when step to perform an action and injects a specific service into the step definition
        /// </summary>
        public static Task<TestScaffold> WhenAsync<TService>(this TestScaffold testScaffold, string description, Func<TService, Task> action) where TService : notnull
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous then step to assert the outcome of the action
        /// </summary>
        public static Task<TestScaffold> ThenAsync(this TestScaffold testScaffold, string description, Func<TestScaffold, Task> action)
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous then step to assert the outcome of the action and injects a specific service into the step definition
        /// </summary>
        public static Task<TestScaffold> ThenAsync<TService>(this TestScaffold testScaffold, string description, Func<TService, Task> action) where TService : notnull
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous and step to add additional steps to your test
        /// </summary>
        public static Task<TestScaffold> AndAsync(this TestScaffold testScaffold, string description, Func<TestScaffold, Task> action)
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(AndAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous and step to add additional steps to your test and injects a specific service into the step definition
        /// </summary>
        public static Task<TestScaffold> AndAsync<TService>(this TestScaffold testScaffold, string description, Func<TService, Task> action) where TService : notnull
        {
            return TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(AndAsync), description, action);
        }

        /// <summary>
        /// Catches an Exception from an asynchronous action and saves it to the TestScaffoldContext for assertion
        /// </summary>
        /// <param name="testScaffold"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task CatchAsync<T>(this TestScaffold testScaffold, Func<Task> action) where T : Exception
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                if (e is T)
                    testScaffold.TestScaffoldContext.Set(e, "CaughtException");
                else throw;
            }
        }

        /// <summary>
        /// Handles a caught exception with an asynchronous handler
        /// </summary>
        /// <param name="testScaffold"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task HandleAsync<T>(this TestScaffold testScaffold, Func<T, Task> action) where T : Exception
        {
            if (!testScaffold.TestScaffoldContext.TryGetValue<T>("CaughtException", out var exception) || exception == null)
                throw new InvalidOperationException($"Handle<{typeof(T).Name}> was called but no exception of type {typeof(T).Name} was caught. Ensure Catch<{typeof(T).Name}> is called before Handle.");
            await action(exception);
        }

        // === Async extensions on Task<TestScaffold> for fluent chaining ===

        /// <summary>
        /// Adds a scenario definition to an async test chain
        /// </summary>
        public static async Task<TestScaffold> ScenarioAsync(this Task<TestScaffold> testScaffoldTask, string description)
        {
            var testScaffold = await testScaffoldTask;
            TestScaffoldBddHelpers.LogStep(testScaffold, nameof(ScenarioAsync), description);
            return testScaffold;
        }

        /// <summary>
        /// Defines an asynchronous given step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> GivenAsync(this Task<TestScaffold> testScaffoldTask, string description, Func<TestScaffold, Task> action)
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous given step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> GivenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Func<TService, Task> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous given step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> GivenAsync(this Task<TestScaffold> testScaffoldTask, string description, Action<TestScaffold> action)
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous given step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> GivenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Action<TService> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(GivenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous when step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> WhenAsync(this Task<TestScaffold> testScaffoldTask, string description, Func<TestScaffold, Task> action)
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous when step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> WhenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Func<TService, Task> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous when step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> WhenAsync(this Task<TestScaffold> testScaffoldTask, string description, Action<TestScaffold> action)
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous when step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> WhenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Action<TService> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(WhenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous then step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> ThenAsync(this Task<TestScaffold> testScaffoldTask, string description, Func<TestScaffold, Task> action)
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous then step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> ThenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Func<TService, Task> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous then step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> ThenAsync(this Task<TestScaffold> testScaffoldTask, string description, Action<TestScaffold> action)
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous then step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> ThenAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Action<TService> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(ThenAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous and step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> AndAsync(this Task<TestScaffold> testScaffoldTask, string description, Func<TestScaffold, Task> action)
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(AndAsync), description, action);
        }

        /// <summary>
        /// Defines an asynchronous and step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> AndAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Func<TService, Task> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return await TestScaffoldBddHelpers.PerformActionAsync(testScaffold, nameof(AndAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous and step in an async test chain
        /// </summary>
        public static async Task<TestScaffold> AndAsync(this Task<TestScaffold> testScaffoldTask, string description, Action<TestScaffold> action)
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(AndAsync), description, action);
        }

        /// <summary>
        /// Defines a synchronous and step in an async test chain and injects a specific service into the step definition
        /// </summary>
        public static async Task<TestScaffold> AndAsync<TService>(this Task<TestScaffold> testScaffoldTask, string description, Action<TService> action) where TService : notnull
        {
            var testScaffold = await testScaffoldTask;
            return TestScaffoldBddHelpers.PerformAction(testScaffold, nameof(AndAsync), description, action);
        }
    }
}
