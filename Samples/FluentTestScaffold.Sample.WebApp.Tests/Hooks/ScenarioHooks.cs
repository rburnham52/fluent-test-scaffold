using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.Core;
using TechTalk.SpecFlow;

namespace FluentTestScaffold.Sample.WebApp.Tests.Hooks;

[Binding]
public static class ScenarioHooks
{
    [BeforeScenario]
    public static void BeforeScenario(ScenarioContext scenarioContext)
    {
        var webApplicationFactory = new SampleWebApplicationFactory();

        var testScaffold = new TestScaffold()
            .UseAspNet<SampleWebApplicationFactory, Program>(webApplicationFactory);

        scenarioContext.Set(testScaffold);
    }

    [AfterScenario]
    public static void AfterScenario(ScenarioContext scenarioContext)
    {
        var testScaffold = scenarioContext.Get<TestScaffold>();

        testScaffold.Dispose();
    }
}
