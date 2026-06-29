namespace FluentTestScaffold.Core;

public interface ITestScaffoldContext
{
    bool TryGetValue<TValue>(out TValue? value);
    bool TryGetValue<TValue>(string? key, out TValue? value);
    void Set<T>(T? data);
    void Set<T>(T? data, string? key);
    void Set<T>(Func<T> func);
    T Get<T>();
    T Get<T>(string? key);
    bool ContainsKey(string key);
    object? this[string? key] { get; set; }
}

public class TestScaffoldContext : Dictionary<string, object?>, ITestScaffoldContext
{
    object? ITestScaffoldContext.this[string? key]
    {
        get => this.GetValueOrDefault(key ?? throw new InvalidOperationException("Key can not be null"));
        set => base[key ?? throw new InvalidOperationException("Key can not be null")] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool TryGetValue<TValue>(out TValue? value)
    {
        return TryGetValue(GetDefaultKey<TValue>(), out value);
    }

    public bool TryGetValue<TValue>(string? key, out TValue? value)
    {
        if (key != null && base.TryGetValue(key, out var result))
        {
            value = TheValueIsAFactoryMethod<TValue>(result) ? CallTheFactoryMethodToGetTheValue<TValue>(result!) : (TValue?)result;
            return true;
        }

        value = default;
        return false;
    }

    private string? GetDefaultKey<T>()
    {
        return typeof(T).FullName;
    }

    public void Set<T>(T? data)
    {
        Set(data, GetDefaultKey<T>());
    }

    public void Set<T>(T? data, string? key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

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
            value = CallTheFactoryMethodToGetTheValue<T>(value!);
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
