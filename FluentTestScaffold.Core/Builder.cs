using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Core;


/// <summary>
/// Base Builder Class used to add context to the Test Scaffold.
/// </summary>
public class Builder<TBuilder> : IBuilder where TBuilder : Builder<TBuilder>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Queue<Action<IServiceProvider>> _buildActions = new();

    protected IServiceProvider ServiceProvider => _serviceProvider;
    protected TestScaffoldContext TestScaffoldContext => _serviceProvider.GetRequiredService<TestScaffoldContext>();

    /// <summary>
    /// Builder Constructor
    /// </summary>
    /// <param name="serviceProvider"></param>
    protected Builder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Enqueue an action to be applied when Build is called. 
    /// </summary>
    /// <param name="action"></param>
    protected void Enqueue(Action<IServiceProvider> action)
    {
        _buildActions.Enqueue(action);
    }

    /// <summary>
    /// Resolves the TestScaffold for the FluentApi context
    /// </summary>
    /// <returns></returns>
    public TestScaffold UsingTestScaffold()
    {
        Build();
        return _serviceProvider.GetRequiredService<TestScaffold>();
    }

    /// <summary>
    /// Resolves a Builder for the FluentApi context
    /// </summary>
    /// <returns></returns>
    public TNewBuilder UsingBuilder<TNewBuilder>() where TNewBuilder : IBuilder
    {
        Build();
        return _serviceProvider.GetRequiredService<TNewBuilder>();
    }

    /// <summary>
    /// Resolves a Builder for the FluentApi context
    /// </summary>
    /// <returns></returns>
    public TBuilder SetTestContext(string key, object value)
    {
        var testContext =  _serviceProvider.GetRequiredService<TestScaffoldContext>();
        testContext[key] = value;
        
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Conditionally applies an action to the builder.
    /// </summary>
    /// <param name="condition">The condition for applying the action</param>
    /// <param name="action">The action used to apply to the Builder</param>
    /// <returns></returns>
    public TBuilder If(bool condition, Action<TBuilder> action) 
    {
        if (condition)
            action((TBuilder)this);
        
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Conditionally applies an action to the builder.
    /// </summary>
    /// <param name="condition">The condition to determine the action</param>
    /// <param name="trueAction">The action used to apply to the Builder when the condition is true</param>
    /// <param name="falseAction">The action used to apply to the Builder when the condition is false</param>
    /// <returns></returns>
    public TBuilder IfElse(bool condition, Action<TBuilder> trueAction, Action<TBuilder> falseAction) 
    {
        if (condition)
            trueAction((TBuilder)this);
        else
            falseAction((TBuilder)this);
        
        return (TBuilder)this;
    }

    /// <summary>
    /// Build the current builder actions and return the TestScaffold context. 
    /// </summary>
    /// <returns></returns>
    public virtual TestScaffold Build()
    {
        while (_buildActions.Count != 0)
        {
            var action = _buildActions.Dequeue();
            action(_serviceProvider);
        }

        return _serviceProvider.GetRequiredService<TestScaffold>();
    }
}

//Required for the UsingBuilder to be able to resolve a new builder while enabling a fluent api
public interface IBuilder
{
    /// <summary>
    /// Resolves the TestScaffold for the FluentApi context
    /// </summary>
    /// <returns></returns>
    TestScaffold UsingTestScaffold();

    /// <summary>
    /// Resolves a Builder for the FluentApi context
    /// </summary>
    /// <returns></returns>
    TNewBuilder UsingBuilder<TNewBuilder>() where TNewBuilder : IBuilder;

    /// <summary>
    /// Build the current builder actions and return the TestScaffold context. 
    /// </summary>
    /// <returns></returns>
    TestScaffold Build();
}