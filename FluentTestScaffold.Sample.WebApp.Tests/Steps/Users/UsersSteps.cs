using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentTestScaffold.AspNetCore;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.WebApp.Model;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace FluentTestScaffold.Sample.WebApp.Tests.Steps.Users;

[Binding]
public class UsersSteps : TestScaffoldStep
{
    public UsersSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
    }
    
    [Given(@"the following user has been registered into the system")]
    public void GivenTheFollowingUserHasBeenRegisteredIntoTheSystem(Table table)
    {
        var user = table.CreateInstance<User>();
        user.Id = Guid.NewGuid();

        TestScaffold.WithData<TestDbContext, User>(user);
        TestScaffold.TestScaffoldContext.Set(user);
    }
    
    [Given(@"the user has logged in")]
    public async Task GivenTheUserHasLoggedIn()
    {
        var user = TestScaffold.TestScaffoldContext.Get<User>();
        
        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();
        
        var response = await httpClient.PostAsJsonAsync<LoginRequest>(
            "/authentication/login",
            new LoginRequest(user.Email, user.Password));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Given(@"the user attempts to sign in with an incorrect password")]
    public async Task GivenTheUserAttemptsToSignInWithAnIncorrectPassword()
    {
        var user = TestScaffold.TestScaffoldContext.Get<User>();
        
        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();
        
        var response = await httpClient.PostAsJsonAsync<LoginRequest>(
            "/authentication/login",
            new LoginRequest(user.Email, $"__INCORRECT_PASSWORD__{Guid.NewGuid()}" ));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [When(@"the user's details are requested")]
    public async Task WhenTheUsersDetailsAreRequested()
    {
        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();

        var response = await httpClient.GetAsync("/user");
        
        TestScaffold.TestScaffoldContext.Set(response);
    }
    
    [Given(@"the user logs out")]
    public async Task GivenTheUserLogsOut()
    {
        var httpClient = TestScaffold.GetWebApplicationHttpClient<SampleWebApplicationFactory, Program>();
        
        var response = await httpClient.PostAsync(
            "/Authentication/logout", null);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }
    
    [Then(@"the system responds with unauthorized")]
    public void ThenTheSystemRespondsWithNotAuthorized()
    {
        var response = TestScaffold.TestScaffoldContext.Get<HttpResponseMessage>();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Then(@"the system responds with the matching user's details")]
    public async Task ThenTheSystemRespondsWithTheMatchingUsersDetails()
    {
        var user = TestScaffold.TestScaffoldContext.Get<User>();
        
        var response = TestScaffold.TestScaffoldContext.Get<HttpResponseMessage>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userDetails = await response.Content.ReadFromJsonAsync<UserDetails>();

        userDetails
            .Should()
            .BeEquivalentTo(
                user, 
                options => options
                    .Excluding(u => u.Password)
                    .Excluding(u => u.ShoppingCart)
                    .WithMapping<User, UserDetails>(u => u.Id, ud => ud.UserId));
    }
    
}