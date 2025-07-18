using FluentTestScaffold.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.EntityFrameworkCore;

public static class TestScaffoldExtensions
{
    public static TestScaffold WithData<TDbContext, TData>(this TestScaffold testScaffold, params TData[] data)
        where TDbContext : DbContext
        where TData : class
    {
        var serviceProvider = testScaffold.ServiceProvider ??
                              throw new InvalidOperationException("A service provider must be initialized");

        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        dbContext.Set<TData>().AddRange(data);

        dbContext.SaveChanges();

        return testScaffold;
    }
}