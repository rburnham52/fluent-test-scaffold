using System.Reflection;
using System.Text.RegularExpressions;

namespace FluentTestScaffold.Core;

public class ConfigOptions
{
    // Regex pattered used to ignore assemblies during auto discovery
    private string[] _ignoredAssemblyRegex = Array.Empty<string>();

    private ICollection<Assembly> _assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };

    public ConfigOptions() { }

    public ConfigOptions(string[] ignoredAssemblyRegex)
    {
        _ignoredAssemblyRegex = ignoredAssemblyRegex;
    }

    /// <summary>
    /// Enables the Auto Discovery for Builders or DataTemplates
    /// </summary>
    public AutoDiscovery AutoDiscovery { get; set; } = AutoDiscovery.None;

    /// <summary>
    /// Used for discovery of Builders and DataTemplates
    /// </summary>
    public ICollection<Assembly> Assemblies
    {
        get => ApplyFilter(_assemblies);
        set => _assemblies = value;
    }

    private ICollection<Assembly> ApplyFilter(ICollection<Assembly> assemblies)
    {
        if (_ignoredAssemblyRegex.Length == 0) return assemblies;
        return assemblies.Where(a => _ignoredAssemblyRegex.Length > 0 && _ignoredAssemblyRegex.All(r => !Regex.IsMatch(a.FullName, r))).ToList();
    }


    /// <summary>
    /// Default Config Options
    /// </summary>
    public static ConfigOptions Default => new(new[] { "Microsoft.*", "System.*" })
    {
        AutoDiscovery = AutoDiscovery.All,
        Assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList()
    };
}