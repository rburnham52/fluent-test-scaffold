namespace FluentTestScaffold.Core;

[Flags]
public enum AutoDiscovery
{
    None = 0,
    Builders = 1,
    DataTemplates = 2,
    All = Builders | DataTemplates
}
