using System;
using FluentTestScaffold.Core;

namespace FluentTestScaffold.Tests.CustomBuilder.DataTemplates;

public class TestScaffoldDataTemplates
{
    public const string TemplateAttributeName = "Set Context From Attribute Name";

    [DataTemplate]
    public TestScaffold SetContextFromTemplate(TestScaffold testScaffold)
    {
        testScaffold.TestScaffoldContext.Set(true, "AppliedByTemplate");
        return testScaffold;
    }

    [DataTemplate(Name = TemplateAttributeName)]
    public TestScaffold TemplateMatchedByAttributeName(TestScaffold testScaffold)
    {
        return SetContextFromTemplate(testScaffold);
    }


    [DataTemplate]
    public TestScaffold SetContextFromTemplateParameter(TestScaffold testScaffold, Guid id)
    {
        testScaffold.TestScaffoldContext.Set(id, "AppliedByTemplateParameter");
        return testScaffold;
    }


    [DataTemplate]
    public TestScaffold SetContextFromTemplateMultipleParameters(TestScaffold testScaffold, int param1, Guid param2, string param3)
    {
        testScaffold.TestScaffoldContext.Set(param1, nameof(param1));
        testScaffold.TestScaffoldContext.Set(param2, nameof(param2));
        testScaffold.TestScaffoldContext.Set(param3, nameof(param3));

        return testScaffold;
    }
}