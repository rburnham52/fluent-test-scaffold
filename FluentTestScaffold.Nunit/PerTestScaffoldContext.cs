using System.Collections.Concurrent;
using FluentTestScaffold.Core;
using NUnit.Framework;

namespace FluentTestScaffold.Nunit;

/// <summary>
/// An alternative implementation of ITestScaffoldContext that uses a Dictionary for storage.
/// This can be used for scenarios requiring different state management strategies.
/// </summary>
public class PerTestScaffoldContext : ITestScaffoldContext
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> _storage = new();

    public bool TryGetValue<TValue>(out TValue? value)
    {
        return TryGetValue(GetDefaultKey<TValue>(), out value);
    }

    public bool TryGetValue<TValue>(string? key, out TValue? value)
    {
        var storage = GetCurrentTestContextStorage();

        if (key != null && storage.TryGetValue(key, out var result))
        {
            value = TheValueIsAFactoryMethod<TValue>(result)
                ? CallTheFactoryMethodToGetTheValue<TValue>(result!)
                : (TValue?)result;
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

        var storage = GetCurrentTestContextStorage();

        storage[key] = data;
    }

    public void Set<T>(Func<T> func)
    {
        var storage = GetCurrentTestContextStorage();

        storage[GetDefaultKey<T>() ?? throw new InvalidOperationException("Key can not be null")] = func;
    }

    public T Get<T>()
    {
        return Get<T>(GetDefaultKey<T>());
    }

    public T Get<T>(string? key)
    {
        var storage = GetCurrentTestContextStorage();

        var value = storage[key ?? throw new ArgumentNullException(nameof(key))];
        if (TheValueIsAFactoryMethod<T>(value))
            value = CallTheFactoryMethodToGetTheValue<T>(value!);
        return (T)value!;
    }

    public bool ContainsKey(string key)
    {
        var storage = GetCurrentTestContextStorage();
        return storage.ContainsKey(key);
    }

    public object? this[string? key]
    {
        get
        {
            return Get<object?>(key);
        }

        set
        {
            Set(value, key);
        }
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

    private ConcurrentDictionary<string, object?> GetCurrentTestContextStorage()
    {
        var testId = TestContext.CurrentContext.Test.ID;

        var storage = _storage.GetOrAdd(testId, _ => new ConcurrentDictionary<string, object?>());

        return storage;
    }
}
