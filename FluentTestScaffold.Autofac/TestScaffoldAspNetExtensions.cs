using Autofac;
using FluentTestScaffold.AspNetCore;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core;

/// <summary>
/// Autofac-specific ASP.NET Core test scaffold extensions.
/// </summary>
public static partial class TestScaffoldExtensions
{
    /// <summary>
    /// Configures the test scaffold for ASP.NET Core integration testing with Autofac
    /// service overrides. The <paramref name="configureAutofac"/> callback runs after
    /// all app-level Autofac module registrations, giving test overrides "last wins" semantics.
    /// </summary>
    /// <typeparam name="TEntryPoint">The entry point class of the web application under test.</typeparam>
    /// <param name="testScaffold">The test scaffold instance.</param>
    /// <param name="configureAutofac">
    /// Action to configure the Autofac <see cref="ContainerBuilder"/> with test overrides.
    /// This callback executes after all app-level <c>ConfigureContainer</c> callbacks.
    /// </param>
    /// <returns>The test scaffold instance for fluent chaining.</returns>
    /// <remarks>
    /// The <paramref name="configureAutofac"/> parameter is intentionally required (not optional)
    /// to avoid CS0121 overload resolution ambiguity with the base
    /// <c>UseAspNet&lt;TEntryPoint&gt;(Action&lt;IServiceCollection&gt;?, Action&lt;IHostBuilder&gt;?)</c> overload
    /// when both packages are referenced. Callers who do not need Autofac overrides should use the
    /// parameterless <c>UseAspNet&lt;TEntryPoint&gt;()</c> from <c>FluentTestScaffold.AspNetCore</c>.
    /// </remarks>
    public static TestScaffold UseAspNet<TEntryPoint>(
        this TestScaffold testScaffold,
        Action<ContainerBuilder> configureAutofac)
        where TEntryPoint : class
    {
        return testScaffold.UseAspNet<TEntryPoint>(
            configureHost: hostBuilder =>
                hostBuilder.ConfigureContainer(configureAutofac));
    }
}
