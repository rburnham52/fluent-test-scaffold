using FluentTestScaffold.Core;
using FluentTestScaffold.Nunit;
using NUnit.Framework;

namespace FluentTestScaffold.Nunit;

public class NunitTestScaffoldLogger: ITestScaffoldLogger
{
    public void Info(string message)
    {
        TestContext.Progress.WriteLine(message);
    }
}