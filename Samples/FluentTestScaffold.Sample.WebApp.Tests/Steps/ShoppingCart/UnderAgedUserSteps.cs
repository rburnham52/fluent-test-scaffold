using System.Net;
using System.Net.Http.Json;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.WebApp.Model;
using TechTalk.SpecFlow;

namespace FluentTestScaffold.Sample.WebApp.Tests.Steps.ShoppingCart;

[Binding]
public class UnderAgedUserSteps : TestScaffoldStep
{
    public UnderAgedUserSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
    }

    [Given(@"the logged in user is under (\d+)")]
    public async Task GivenTheLoggedInUserIsUnder(int age)
    {
        var user = CreateUser(age - 1);

        var httpCLient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();

        var response = await httpCLient.PostAsJsonAsync(
            "/Authentication/login",
            new LoginRequest(user.Email, user.Password));

        response.EnsureSuccessStatusCode();
    }

    [Given(@"the logged in user is (\d+)")]
    public async Task GivenTheLoggedInUserIs(int age)
    {
        var user = CreateUser(age);

        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();

        var response = await httpClient.PostAsJsonAsync(
            "/Authentication/login",
            new LoginRequest(user.Email, user.Password));

        response.EnsureSuccessStatusCode();
    }

    [Given(@"the logged in user is over (.*)")]
    public async Task GivenTheLoggedInUserIsOver(int age)
    {
        var user = CreateUser(age + 1);

        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();

        var response = await httpClient.PostAsJsonAsync(
            "/Authentication/login",
            new LoginRequest(user.Email, user.Password));

        response.EnsureSuccessStatusCode();
    }

    private User CreateUser(int age)
    {
        var user = new User(
            Guid.NewGuid(),
            "John Doe",
            "john.doe@email.com",
            "Password123!",
            DateTime.Today.AddYears(-age));

        TestScaffold.WithData<TestDbContext, User>(user);

        TestScaffold.TestScaffoldContext.Set(user);
        return user;
    }
}
