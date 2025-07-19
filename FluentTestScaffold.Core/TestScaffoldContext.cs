namespace FluentTestScaffold.Core;

public class TestScaffoldContext : Dictionary<string, object>
{
    public bool TryGetValue<TValue>(out TValue? value)
    {
        return TryGetValue(GetDefaultKey<TValue>(), out value);
    }

    public bool TryGetValue<TValue>(string? key, out TValue? value)
    {
        if (key != null && base.TryGetValue(key, out var result))
        {
            value = TheValueIsAFactoryMethod<TValue>(result) ? CallTheFactoryMethodToGetTheValue<TValue>(result) : (TValue)result;
            return true;
        }

        value = default;
        return false;
    }

    private string? GetDefaultKey<T>()
    {
        return typeof(T).FullName;
    }

    public void Set<T>(T data)
    {
        Set(data, GetDefaultKey<T>());
    }

    public void Set<T>(T data, string? key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));
        if (data != null)
            this[key] = data;
    }

    public void Set<T>(Func<T> func)
    {
        this[GetDefaultKey<T>() ?? throw new InvalidOperationException("Key can not be null")] = func;
    }

    public T Get<T>()
    {
        return Get<T>(GetDefaultKey<T>());
    }

    public T Get<T>(string? key)
    {
        var value = this[key ?? throw new ArgumentNullException(nameof(key))];
        if (TheValueIsAFactoryMethod<T>(value))
            value = CallTheFactoryMethodToGetTheValue<T>(value);
        return (T)value!;
    }

    private static T CallTheFactoryMethodToGetTheValue<T>(object value)
    {
        return ((Func<T>)value)();
    }

    private static bool TheValueIsAFactoryMethod<T>(object? value)
    {
        if (value == null) return false;
        return value is Func<T>;
    }
}
