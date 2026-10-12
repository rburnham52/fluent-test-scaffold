using System;
using FluentAssertions;
using FluentTestScaffold.Core;
using FluentTestScaffold.Nunit;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[Parallelizable(ParallelScope.Children)]
public class TestContextPerTestScaffoldContextTests
{
    private readonly TestScaffold _testScaffold = new TestScaffold(new PerTestScaffoldContext());

    [Test]
    public void CanSetContextPerTest()
    {
        _testScaffold.TestScaffoldContext.Set<string>("Test");

        _testScaffold.TestScaffoldContext.Get<string>()
            .Should()
            .Be("Test");
    }

    [TestCase("Apple")]
    [TestCase("Banana")]
    [TestCase("Pear")]
    public void CanSetContextPerTestWhenRunInParallel(string testData)
    {
        _testScaffold.TestScaffoldContext.Set<string>(testData);

        _testScaffold.TestScaffoldContext.ContainsKey(typeof(string).FullName!).Should().BeTrue();

        _testScaffold.TestScaffoldContext.Get<string>()
            .Should()
            .Be(testData);

        _testScaffold.TestScaffoldContext[typeof(string).FullName!]
            .Should()
            .Be(testData);
    }

    [Test]
    public void TryGetResolvesExistingValueByType()
    {
        _testScaffold.TestScaffoldContext.Set("Test Value");

        _testScaffold.TestScaffoldContext.TryGetValue<string>(out var value)
            .Should()
            .BeTrue();

        value.Should().Be("Test Value");

    }

    [Test]
    public void TryGetResolvesExistingValueByKey()
    {
        _testScaffold.TestScaffoldContext.Set("Test Value", "Key 1");

        _testScaffold.TestScaffoldContext.TryGetValue<string>("Key 1", out var value)
            .Should()
            .BeTrue();

        value.Should().Be("Test Value");
    }

    [Test]
    public void TryGetOnUnknownKey()
    {
        _testScaffold.TestScaffoldContext.Set("Test Value", "Key 1");

        _testScaffold.TestScaffoldContext.TryGetValue<string>("Key XXX", out var value)
            .Should()
            .BeFalse();

        value.Should().Be(null);
    }

    [Test]
    public void ThrowsExceptionWhenKeyIsNull()
    {
        Action act = () => _testScaffold.TestScaffoldContext.Set("Test Value", null);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CanSetValueFromFunction()
    {
        _testScaffold.TestScaffoldContext.Set(() => "Test Value");

        _testScaffold.TestScaffoldContext.TryGetValue<string>(out var value)
            .Should()
            .BeTrue();

        value.Should().Be("Test Value");

        _testScaffold.TestScaffoldContext.Get<string>()
            .Should()
            .Be("Test Value");
    }

    [Test]
    public void CanDirectlySetKeyValue()
    {
        _testScaffold.TestScaffoldContext["Key 1"] = "Test Value";

        var value = _testScaffold.TestScaffoldContext["Key 1"];

        value.Should().Be("Test Value");
    }

    [Test]
    public void ThrowsExceptionWhenSettingWithNullKey()
    {
        Action act = () => _testScaffold.TestScaffoldContext[null] = "Test Value";

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CanStoreNullValue()
    {
        _testScaffold.TestScaffoldContext["Key 1"] = null;

        var value = _testScaffold.TestScaffoldContext["Key 1"];
        value.Should().BeNull();
    }
}
