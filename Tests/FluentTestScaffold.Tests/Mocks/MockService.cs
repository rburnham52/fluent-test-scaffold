using System;
using System.Collections.Generic;
using FluentTestScaffold.Core;

namespace FluentTestScaffold.Tests.Mocks;

class MockService : IMockService
{
    public int Count { get; set; } = 0;

    public void Increment()
    {
        Count++;
    }
}

interface IMockService
{
    int Count { get; set; }
    void Increment();
}

public class MockBuilder : Builder<MockBuilder>
{
    public MockBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public MockBuilder WithMockData(List<string> messages)
    {
        messages.Add("Hello World!");
        return this;
    }
}