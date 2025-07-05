using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TechTalk.SpecFlow;

namespace FluentTestScaffold.Sample.WebApp.Tests.Steps.Common;

[Binding]
public class WebRequestSteps : TestScaffoldStep
{
    public WebRequestSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
    }
    
    [Then(@"request should be successful")]
    public void ThenRequestShouldBeSuccessful()
    {
        var httpResponseMessage = TestScaffold.TestScaffoldContext.Get<HttpResponseMessage>();

        httpResponseMessage.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Then(@"the request should fail")]
    public void ThenTheRequestShouldFail()
    {
        var httpResponseMessage = TestScaffold.TestScaffoldContext.Get<HttpResponseMessage>();

        httpResponseMessage.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Then(@"the request should fail with message ""(.*)""")]
    public async Task ThenTheRequestShouldFailWithMessage(string errorMessage)
    {
        var httpResponseMessage = TestScaffold.TestScaffoldContext.Get<HttpResponseMessage>();

        httpResponseMessage.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        var problem = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();

        problem!.Detail.Should().Be(errorMessage);
    }
}