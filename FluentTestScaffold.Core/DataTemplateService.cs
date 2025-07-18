using System.Reflection;

namespace FluentTestScaffold.Core;

public class DataTemplateService
{
    private readonly ICollection<MethodInfo> _dataTemplateMethodInfo;

    public DataTemplateService(ICollection<MethodInfo> dataTemplateMethodInfo)
    {
        _dataTemplateMethodInfo = dataTemplateMethodInfo;
    }

    /// <summary>
    /// Finds a Data Template MethodInfo by Name.
    /// </summary>
    /// <param name="templateName">Matched against the the Method Name or Attribute Name Property</param>
    /// <returns></returns>
    /// <exception cref="MissingMethodException">Thrown when no matching DataTemplate method could be found</exception>
    /// <exception cref="InvalidOperationException">Thrown where then was a problem Invoking the matched method</exception>
    public MethodInfo FindByName(string templateName)
    {
        var methods = _dataTemplateMethodInfo
            .Where(m => m.Name == templateName || m.GetCustomAttributes(typeof(DataTemplateAttribute), false).Any(a => (a as DataTemplateAttribute)?.Name == templateName))
            .ToArray();
        if (methods.Length == 0)
            throw new MissingMethodException(
                $"Could not find DataTemplate method matching the name {templateName} that takes a {nameof(TestScaffold)} as a parameter");
        if (methods.Length > 1)
            throw new InvalidOperationException($"Found more than one DataTemplate matching the name {templateName}");

        return methods[0];
    }
}