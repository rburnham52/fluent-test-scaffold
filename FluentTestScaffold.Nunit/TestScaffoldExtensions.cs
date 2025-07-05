using FluentTestScaffold.Nunit;

// ReSharper disable once CheckNamespace
namespace FluentTestScaffold.Core
{
    public static class TestScaffoldExtensions
    {
        public static TestScaffold UsingNunit(this TestScaffold testScaffold)
        {
            DefaultLogger.Logger = new NunitTestScaffoldLogger();
            return testScaffold;
        }
    
    }
}
