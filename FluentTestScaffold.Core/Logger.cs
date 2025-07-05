namespace FluentTestScaffold.Core;

public interface ITestScaffoldLogger
{
    public void Info(string message);     
}

public static class DefaultLogger 
{
    public static ITestScaffoldLogger Logger { get; set; } = new ConsoleLogger();
}

public class ConsoleLogger: ITestScaffoldLogger
{
    public void Info(string message) => Console.WriteLine(message);
}