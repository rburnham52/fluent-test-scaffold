using System;
using System.Collections.Generic;
using FluentTestScaffold.Core;
using FluentTestScaffold.Tests.Mocks;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core;

[TestFixture]
public class TypeExtensionsTests
{
    [TestCase(typeof(List<int>), typeof(IEnumerable<>), true, TestName = "Implements generic interface")]
    [TestCase(typeof(List<int>), typeof(List<>), true, TestName = "Is the generic type")]
    [TestCase(typeof(MockBuilder), typeof(Builder<>), true, TestName = "Inherits from the generic type")]
    [TestCase(typeof(string), typeof(Builder<>), false, TestName = "Unrelated type")]
    [TestCase(typeof(List<int>), typeof(Dictionary<,>), false, TestName = "Unrelated generic type")]
    [TestCase(typeof(IList<int>), typeof(Builder<>), false, TestName = "Interface with no base type")]
    public void IsAssignableToGenericType(Type givenType, Type genericType, bool expected)
    {
        Assert.AreEqual(expected, givenType.IsAssignableToGenericType(genericType));
    }
}
