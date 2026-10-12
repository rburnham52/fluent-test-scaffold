using System;
using System.Linq;
using FluentTestScaffold.Core;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class EntityFrameworkCoreTestScaffoldExtensionsTests
{
    private static User CreateUser(string name) =>
        new(Guid.NewGuid(), name, $"{name}@test.com", "SuperSecret", new DateTime(1990, 03, 30));

    [Test]
    public void WithData_Saves_Data_To_DbContext()
    {
        var dbContext = TestDbContextFactory.Create();
        var john = CreateUser("John");
        var jane = CreateUser("Jane");

        using var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton(dbContext));

        var result = testScaffold.WithData<TestDbContext, User>(john, jane);

        Assert.AreSame(testScaffold, result);
        CollectionAssert.AreEquivalent(
            new[] { john.Id, jane.Id },
            dbContext.Users.Select(u => u.Id).ToArray());
    }

    [Test]
    public void WithData_Without_Data_Saves_Nothing()
    {
        var dbContext = TestDbContextFactory.Create();

        using var testScaffold = new TestScaffold()
            .UseIoc(ctx => ctx.Container.AddSingleton(dbContext));

        testScaffold.WithData<TestDbContext, User>();

        Assert.IsEmpty(dbContext.Users);
    }

    [Test]
    public void WithData_Without_ServiceProvider_Throws()
    {
        using var testScaffold = new TestScaffold();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            testScaffold.WithData<TestDbContext, User>(CreateUser("John")));

        Assert.That(exception!.Message, Does.Contain("service provider"));
    }

    [Test]
    public void WithData_Without_Registered_DbContext_Throws()
    {
        using var testScaffold = new TestScaffold()
            .UseIoc();

        Assert.Throws<InvalidOperationException>(() =>
            testScaffold.WithData<TestDbContext, User>(CreateUser("John")));
    }
}
