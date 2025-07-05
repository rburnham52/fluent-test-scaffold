using FluentTestScaffold.Core;
using TechTalk.SpecFlow;

namespace FluentTestScaffold.Sample.WebApp.Tests.Steps;

public class TestScaffoldStep
{
    public ScenarioContext ScenarioContext { get; }
    
    public TestScaffold TestScaffold { get; }
    
    public SampleWebApplicationFactory SampleWebApplicationFactory { get; }
    
    public TestScaffoldStep(ScenarioContext scenarioContext)
    {
        ScenarioContext = scenarioContext;

        TestScaffold = ScenarioContext.Get<TestScaffold>();

        SampleWebApplicationFactory = TestScaffold.TestScaffoldContext.Get<SampleWebApplicationFactory>();
    }
}