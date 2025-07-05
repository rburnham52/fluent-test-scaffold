using FluentTestScaffold.Core;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Tests.CustomBuilder;
using TechTalk.SpecFlow.Assist;

namespace FluentTestScaffold.Specflow.Tests.Steps;

[Binding]
public class UserSteps
{
    private readonly ScenarioContext _scenarioContext;

    public UserSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I am Authenticated as the User")]
    public void GivenIAmAnAuthenticatedAUser(Table table)
    {
        var user = table.CreateInstance<User>();
        user.Id = Defaults.CurrentUserId;
        var testScaffold = _scenarioContext.Get<TestScaffold>(nameof(TestScaffold));


        testScaffold.UsingBuilder<UserBuilder>()
            .With(user)
            .WithShoppingCart(user.Id)
            .Build();

        var authService = testScaffold.Resolve<IAuthService>();
        authService.AuthenticateUser(user.Email, user.Password);

        _scenarioContext.Add("CurrentUserId", user.Id);
    }
}