namespace FluentTestScaffold.Core;

public static class Merger
{
    /// <summary>
    /// Merges non default values into an existing object of the same type and returns a new instance
    /// </summary>
    /// <param name="baseObject">The base object that will be merged into</param>
    /// <param name="overrideObject">The source object that will be used to override values</param>
    /// <returns></returns>
    public static T CloneAndMerge<T>(T baseObject, T overrideObject) where T : new()
    {
        var t = typeof(T);
        var publicProperties = t.GetProperties();
        
        var output = new T();
        
        foreach (var propInfo in publicProperties)
        {
            var overrideValue = propInfo.GetValue(overrideObject);
            var defaultValue = !propInfo.PropertyType.IsValueType 
                ? null 
                : Activator.CreateInstance(propInfo.PropertyType);
            if (overrideValue == defaultValue)
            {
                propInfo.SetValue(output, propInfo.GetValue(baseObject));   
            }
            else 
            {
                propInfo.SetValue(output, overrideValue);
            }
        }
        
        return output;
    }
}