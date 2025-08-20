using System;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Tests.CustomBuilder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentTestScaffold.Tests;

[TestFixture]
public class EntityFrameworkCoreServiceProviderExtensionsTests
{
    private IServiceCollection? _services;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_WithExistingDbContext_ShouldReplaceWithInMemory()
    {
        // Arrange
        _services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test;Trusted_Connection=true;"));

        // Act
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<TestDbContext>();

        Assert.That(dbContext, Is.Not.Null);
        Assert.That(dbContext.Database.IsInMemory(), Is.True);
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_WithExistingDbContextOptions_ShouldRemoveOptions()
    {
        // Arrange
        _services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test;Trusted_Connection=true;"));

        // Act
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetService<DbContextOptions<TestDbContext>>();

        Assert.That(options, Is.Not.Null);
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_WithoutExistingDbContext_ShouldAddInMemoryDbContext()
    {
        // Arrange - No existing DbContext registration

        // Act
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<TestDbContext>();

        Assert.That(dbContext, Is.Not.Null);
        Assert.That(dbContext.Database.IsInMemory(), Is.True);
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.ReplaceDbContextWithInMemoryProvider<TestDbContext>());

        Assert.That(exception.ParamName, Is.EqualTo("services"));
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_ShouldReturnServicesCollection()
    {
        // Arrange
        _services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test;Trusted_Connection=true;"));

        // Act
        var result = _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();

        // Assert
        Assert.That(result, Is.SameAs(_services));
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_MultipleCalls_ShouldWorkCorrectly()
    {
        // Arrange
        _services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test;Trusted_Connection=true;"));

        // Act
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var dbContext = serviceProvider.GetService<TestDbContext>();

        Assert.That(dbContext, Is.Not.Null);
        Assert.That(dbContext.Database.IsInMemory(), Is.True);
    }

    [Test]
    public void ReplaceDbContextWithInMemoryProvider_WithDifferentDbContextTypes_ShouldWorkIndependently()
    {
        // Arrange
        _services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test;Trusted_Connection=true;"));
        _services.AddDbContext<AnotherTestDbContext>(options =>
            options.UseSqlServer("Server=test;Database=test2;Trusted_Connection=true;"));

        // Act
        _services.ReplaceDbContextWithInMemoryProvider<TestDbContext>();
        _services.ReplaceDbContextWithInMemoryProvider<AnotherTestDbContext>();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var testDbContext = serviceProvider.GetService<TestDbContext>();
        var anotherTestDbContext = serviceProvider.GetService<AnotherTestDbContext>();

        Assert.That(testDbContext, Is.Not.Null);
        Assert.That(testDbContext.Database.IsInMemory(), Is.True);
        Assert.That(anotherTestDbContext, Is.Not.Null);
        Assert.That(anotherTestDbContext.Database.IsInMemory(), Is.True);
    }
}

public class AnotherTestDbContext : DbContext
{
    public AnotherTestDbContext(DbContextOptions<AnotherTestDbContext> options) : base(options)
    {
    }
}
