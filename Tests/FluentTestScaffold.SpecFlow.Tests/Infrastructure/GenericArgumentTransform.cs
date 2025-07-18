// using FluentTestScaffold.Core;
// using TechTalk.SpecFlow.Assist;
//
// namespace FluentTestScaffold.Specflow.Tests.Infrastructure;
//
// [Binding]
// public class GenericArgumentTransform
// {
//     private readonly ScenarioContext _scenarioContext;
//
//     public GenericArgumentTransform(ScenarioContext scenarioContext)
//     {
//         _scenarioContext = scenarioContext;
//     }
//
//     // [StepArgumentTransformation(@"db:(.*)")]
//     // public object GenerateEmployee(Table table, string type)
//     // {
//     //     var testScaffold = _scenarioContext.Get<TestScaffold>(nameof(TestScaffold));
//     //
//     //     var config = testScaffold.Resolve<SpecflowBuilderConfig>();
//     //
//     //     var type = config.Assembly?.
//     //     var u1 = table.CreateInstance<AuthManagerSteps.TestEmployee>();
//     //
//     //     //use mock data for other properties
//     //     var user = new Fixture()
//     //         .Build<AuthManagerSteps.TestEmployee>()
//     //         .With(u => u.FirstName, u1.FirstName)
//     //         .With(u => u.Surname, u1.Surname);
//     //
//     //     return user.Create();
//     // }
// }
