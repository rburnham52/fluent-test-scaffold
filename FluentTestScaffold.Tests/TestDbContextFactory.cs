using System;
using FluentTestScaffold.Sample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FluentTestScaffold.Tests;

public static class TestDbContextFactory
{
    public static TestDbContext Create()
    {
        var contextOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var dbContext = new TestDbContext(contextOptions);

        return dbContext;
    }
}