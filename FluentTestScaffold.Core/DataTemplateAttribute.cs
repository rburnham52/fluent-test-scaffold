namespace FluentTestScaffold.Core;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate)]
public class DataTemplateAttribute : Attribute
{
    /// <summary>
    /// Used as an alternative to matching by Method Name
    /// </summary>
    public string? Name { get; set; }
}