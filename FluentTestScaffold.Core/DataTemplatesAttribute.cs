namespace FluentTestScaffold.Core;

[AttributeUsage(AttributeTargets.Class)]
public class DataTemplatesAttribute : Attribute
{
    /// <summary>
    /// Optional name for the template class
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional description for the template class
    /// </summary>
    public string? Description { get; set; }
}
