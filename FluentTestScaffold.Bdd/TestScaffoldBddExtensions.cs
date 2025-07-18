using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core
{
    /// <summary>
    /// Extension methods to support BDD style Tests with the Test Scaffold
    /// </summary>
    public static class TestScaffoldBddExtensions
    {

        /// <summary>
        /// Adds a scenario definition to your test
        /// </summary>
        public static TestScaffold Scenario(this TestScaffold testScaffold, string description)
        {
            LogStep(testScaffold, nameof(Scenario), description);
            return testScaffold;
        }
        /// <summary>
        /// Defines a given step to prepare your test state
        /// </summary>
        public static TestScaffold Given(this TestScaffold testScaffold, string description, Action<TestScaffold> action)
        {
            return PerformAction(testScaffold, nameof(Given), description, action);
        }

        /// <summary>
        /// Defines a given step to prepare your test state and injects a specific service into the step definition
        /// </summary>
        public static TestScaffold Given<TService>(this TestScaffold testScaffold, string description, Action<TService> action) where TService : notnull
        {
            return PerformAction(testScaffold, nameof(Given), description, action);
        }

        /// <summary>
        /// Defines a when step to perform an action
        /// </summary>
        public static TestScaffold When(this TestScaffold testScaffold, string description, Action<TestScaffold> action)
        {
            return PerformAction(testScaffold, nameof(When), description, action);
        }

        /// <summary>
        /// Defines a when step to perform an action and injects a specific service into the step definition
        /// </summary>
        public static TestScaffold When<TService>(this TestScaffold testScaffold, string description, Action<TService> action) where TService : notnull
        {
            return PerformAction(testScaffold, nameof(When), description, action);
        }

        /// <summary>
        /// Defines a then step to assert the outcome of the action
        /// </summary>
        public static TestScaffold Then(this TestScaffold testScaffold, string description, Action<TestScaffold> action)
        {
            return PerformAction(testScaffold, nameof(Then), description, action);
        }

        /// <summary>
        /// Defines a then step to assert the outcome of the action and injects a specific service into the step definition
        /// </summary>
        public static TestScaffold Then<TService>(this TestScaffold testScaffold, string description, Action<TService> action) where TService : notnull
        {
            return PerformAction(testScaffold, nameof(Then), description, action);
        }


        /// <summary>
        /// Defines an and step to add additional steps to your test
        /// </summary>
        public static TestScaffold And(this TestScaffold testScaffold, string description, Action<TestScaffold> action)
        {
            return PerformAction(testScaffold, nameof(And), description, action);
        }

        /// <summary>
        /// Defines an and step to add additional steps to your test and injects a specific service into the step definition
        /// </summary>
        public static TestScaffold And<TService>(this TestScaffold testScaffold, string description, Action<TService> action) where TService : notnull
        {
            return PerformAction(testScaffold, nameof(And), description, action);
        }

        /// <summary>
        /// Catches an Exception and allows and saves it to the TestScaffoldContext for assertion
        /// </summary>
        /// <param name="testScaffold"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Catch<T>(this TestScaffold testScaffold, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e is T)
                    testScaffold.TestScaffoldContext.Set(e, "CaughtException");
                else throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="testScaffold"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Handle<T>(this TestScaffold testScaffold, Action<T> action)
        {
            var exception = testScaffold.TestScaffoldContext.Get<T>("CaughtException");
            if (exception != null)
                action(exception);
        }


        private static TestScaffold PerformAction(TestScaffold testScaffold, string actionType, string description, Action<TestScaffold> action)
        {
            var logger = testScaffold.ServiceProvider?.GetService<ITestScaffoldLogger>();
            if (logger != null)
                logger.Info(actionType + " " + description);

            action(testScaffold);
            return testScaffold;
        }

        private static TestScaffold PerformAction<TService>(TestScaffold testScaffold, string actionType, string description, Action<TService> action) where TService : notnull
        {
            LogStep(testScaffold, actionType, description);

            var service = testScaffold.Resolve<TService>();
            action(service);
            return testScaffold;
        }

        private static void LogStep(TestScaffold testScaffold, string actionType, string description)
        {
            var logger = testScaffold.ServiceProvider?.GetService<ITestScaffoldLogger>();
            if (logger != null)
                logger.Info(actionType + " " + description);
        }
    }
}
