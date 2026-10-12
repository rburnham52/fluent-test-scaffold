using System;
using System.Collections.Generic;
using FluentTestScaffold.Core;
using NUnit.Framework;

namespace FluentTestScaffold.Tests.Core
{
    [TestFixture]
    public class TestContextTests
    {
        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void SetAndGetValueWithDefaultKey<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(value);
            T retrievedValue = context.Get<T>();
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void SetAndGetValueWithCustomKey<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(value, "myKey");
            T retrievedValue = context.Get<T>("myKey");
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void SetAndGetValueWithFactoryMethod<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(() => value);
            T retrievedValue = context.Get<T>();
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void SetAndGetValueWithFactoryMethodAndCustomKey<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(() => value, "myKey");
            T retrievedValue = context.Get<T>("myKey");
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void TryGetValueWithDefaultValue<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(value);
            bool success = context.TryGetValue<T>(out var retrievedValue);
            Assert.IsTrue(success);
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void TryGetValueWithCustomKey<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(value, "myKey");
            bool success = context.TryGetValue<T>("myKey", out var retrievedValue);
            Assert.IsTrue(success);
            Assert.AreEqual(value, retrievedValue);
        }

        [TestCase(42)]
        [TestCase("Hello, World!")]
        [TestCase(3.14)]
        [TestCase(true)]
        public void TryGetValueWithFactoryMethod<T>(T value)
        {
            var context = new TestScaffoldContext();
            context.Set(() => value);
            bool success = context.TryGetValue<T>(out var retrievedValue);
            Assert.IsTrue(success);
            Assert.AreEqual(value, retrievedValue);
        }

        [Test]
        public void TryGetValueWithNonExistentKey()
        {
            var context = new TestScaffoldContext();
            bool success = context.TryGetValue(out int value);
            Assert.IsFalse(success);
        }

        [Test]
        public void TryGetValueWithNonExistentKeyAndFactoryMethod()
        {
            var context = new TestScaffoldContext();
            context.Set(() => 42);
            bool success = context.TryGetValue("myKey", out int value);
            Assert.IsFalse(success);
        }

        [Test]
        public void TryGetValueWithNonExistentKeyAndDefaultValue()
        {
            var context = new TestScaffoldContext();
            bool success = context.TryGetValue("myKey", out int value);
            Assert.IsFalse(success);
        }

        [Test]
        public void TryGetValueWithNullKey()
        {
            var context = new TestScaffoldContext();
            context.Set(42);
            bool success = context.TryGetValue(null, out int value);
            Assert.IsFalse(success);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void TryGetValueWithFactoryMethodAndCustomKey()
        {
            var context = new TestScaffoldContext { ["myKey"] = new Func<int>(() => 42) };
            bool success = context.TryGetValue("myKey", out int value);
            Assert.IsTrue(success);
            Assert.AreEqual(42, value);
        }

        [Test]
        public void SetNullValueDoesNotStoreValue()
        {
            var context = new TestScaffoldContext();
            context.Set<string?>(null, "myKey");
            Assert.IsFalse(context.ContainsKey("myKey"));
        }

        [Test]
        public void SetOverwritesExistingValue()
        {
            var context = new TestScaffoldContext();
            context.Set(1);
            context.Set(2);
            Assert.AreEqual(2, context.Get<int>());
        }

        [Test]
        public void SetFactoryMethodIsEvaluatedOnEachGet()
        {
            var count = 0;
            var context = new TestScaffoldContext();
            context.Set(() => ++count);

            Assert.AreEqual(0, count);
            Assert.AreEqual(1, context.Get<int>());
            Assert.AreEqual(2, context.Get<int>());
        }

        [Test]
        public void GetWithNonExistentKeyThrowsException()
        {
            var context = new TestScaffoldContext();
            Assert.Throws<KeyNotFoundException>(() => context.Get<int>());
        }

        [Test]
        public void SetNullKeyThrowsException()
        {
            var context = new TestScaffoldContext();
            Assert.Throws<ArgumentNullException>(() => context.Set(42, null));
        }

        [Test]
        public void GetWithNullKeyThrowsException()
        {
            var context = new TestScaffoldContext();
            Assert.Throws<ArgumentNullException>(() => context.Get<int>(null));
        }
    }
}
