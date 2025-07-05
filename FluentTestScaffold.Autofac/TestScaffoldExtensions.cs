using Autofac;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core;

/// <summary>
/// TestScaffold extensions
/// </summary>
public static class TestScaffoldExtensions
{
    /// <summary>
    /// Configures Test Scaffold context.
    /// </summary>
    /// <param name="testScaffold"></param>
    /// <param name="configureServices"></param>
    /// <returns></returns>
    public static TestScaffold UseAutofac(this TestScaffold testScaffold, Action<AutofacServiceBuilder>? configureServices = null)
    {
        testScaffold.UseIoc(new AutofacServiceBuilder(testScaffold.Options), configureServices);
        return testScaffold;
    }

    /// <summary>
    /// Configures Test Scaffold context.
    /// </summary>
    /// <param name="testScaffold"></param>
    /// <param name="serviceBuilder"></param>
    /// <param name="configureServices"></param>
    /// <returns></returns>
    public static TestScaffold UseAutofac<TServiceBuilder>(
        this TestScaffold testScaffold,
        TServiceBuilder serviceBuilder,
        Action<TServiceBuilder>? configureServices = null)
    where TServiceBuilder : IocServiceBuilder<ContainerBuilder, TServiceBuilder>
    {
        testScaffold.UseIoc(serviceBuilder, configureServices);
        return testScaffold;
    }
}