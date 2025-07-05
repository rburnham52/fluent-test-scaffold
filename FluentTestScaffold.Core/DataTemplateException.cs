using System.Reflection;

namespace FluentTestScaffold.Core;

public class DataTemplateException: Exception
{
    public DataTemplateException(string templateName, Exception innerException) : base($"Failed to apply template '{templateName}'. {innerException.Message}'", innerException) { }
}