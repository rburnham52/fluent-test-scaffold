using System;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.Mocks;

namespace FluentTestScaffold.Tests.CustomBuilder.DataTemplates;

[DataTemplates]
public class TestScaffoldDataTemplates
{
    private readonly TestScaffold _testScaffold;
    private readonly MockService? _mockService;
    private readonly object? _testValue;

    public TestScaffoldDataTemplates(TestScaffold testScaffold)
    {
        _testScaffold = testScaffold;
    }

    // Constructor that actually uses injected services
    public TestScaffoldDataTemplates(TestScaffold testScaffold, MockService mockService, object testValue)
    {
        _testScaffold = testScaffold;
        _mockService = mockService;
        _testValue = testValue;
    }

    public void SetContextFromTemplate()
    {
        _testScaffold.TestScaffoldContext.Set(true, "AppliedByTemplate");
    }

    public void TemplateMatchedByAttributeName()
    {
        _testScaffold.TestScaffoldContext.Set(true, "AppliedByTemplateAttributeName");
    }

    public void SetContextFromTemplateParameter(Guid id)
    {
        _testScaffold.TestScaffoldContext.Set(id, "AppliedByTemplateParameter");
    }

    public void SetContextFromTemplateMultipleParameters(int param1, Guid param2, string param3)
    {
        _testScaffold.TestScaffoldContext.Set(param1, nameof(param1));
        _testScaffold.TestScaffoldContext.Set(param2, nameof(param2));
        _testScaffold.TestScaffoldContext.Set(param3, nameof(param3));
    }

    // Method that actually uses the injected services
    public void SetContextFromTemplateWithInjectedServices()
    {
        // Verify the injected services are actually available
        if (_mockService == null)
        {
            throw new InvalidOperationException("MockService was not injected");
        }

        if (_testValue == null)
        {
            throw new InvalidOperationException("TestValue was not injected");
        }

        // Use the injected services to set context
        _testScaffold.TestScaffoldContext.Set(_mockService.GetType().Name, "InjectedServiceType");
        _testScaffold.TestScaffoldContext.Set(_testValue, "InjectedTestValue");
        _testScaffold.TestScaffoldContext.Set(true, "AppliedByTemplateWithInjectedServices");
    }
}