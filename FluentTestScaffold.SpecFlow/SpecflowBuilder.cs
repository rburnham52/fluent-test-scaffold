// ReSharper disable once CheckNamespace

namespace FluentTestScaffold.Core;

public class SpecflowBuilder : Builder
{
    public SpecflowBuilder(TestScaffold testScaffold) : base(testScaffold)
    {
    }
    //
    // public EfCoreBuilder<TDbContext> With<TEntity>(TEntity entity)
    // {
    //     Enqueue(() =>
    //     {
    //         if (entity == null) return;
    //         _dbContext.Add(entity);
    //         _dbContext.SaveChanges();
    //     });
    //
    //     return this;
    // }
    //
    // public EfCoreBuilder<TDbContext> WithRange<T>(IEnumerable<T> entities)
    // {
    //     Enqueue(() =>
    //     {
    //         _dbContext.AddRange((IEnumerable<object>)entities);
    //         _dbContext.SaveChanges();
    //     });
    //     return this;
    // }
}