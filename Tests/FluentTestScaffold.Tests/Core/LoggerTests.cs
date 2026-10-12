using System;
using System.IO;
using FluentTestScaffold.Core;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class LoggerTests
{
    [Test]
    public void ConsoleLogger_Info_Writes_Message_To_Console()
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            new ConsoleLogger().Info("Hello World!");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.AreEqual("Hello World!" + Environment.NewLine, writer.ToString());
    }
}
