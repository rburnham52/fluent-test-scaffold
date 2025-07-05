using FluentTestScaffold.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.EntityFrameworkCore;

/// <summary>
/// Base builder for builder up an entity framework DbContext
/// </summary>
/// <typeparam name="TDbContext">The DBContext to build</typeparam>
public class EfCoreBuilder<TDbContext> : EfCoreBuilder<TDbContext, EfCoreBuilder<TDbContext>>
    where TDbContext : DbContext
{
    /// <summary>
    /// Base builder for builder up an entity framework DbContext
    /// </summary>
    /// <param name="serviceProvider">Will be injected from the TestScaffold Ioc</param>
    public EfCoreBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}

/// <summary>
/// Base builder for custom builder implementations. 
/// </summary>
/// <typeparam name="TDbContext">The DBContext to build</typeparam>
/// <typeparam name="TBuilder">The type of the Custom Builder</typeparam>
public class EfCoreBuilder<TDbContext, TBuilder> : Builder<TBuilder>
    where TDbContext : DbContext
    where TBuilder : EfCoreBuilder<TDbContext, TBuilder>
{
    /// <summary>
    /// Base builder for builder up an entity framework DbContext
    /// </summary>
    /// <param name="serviceProvider">Will be injected from the TestScaffold Ioc</param>
    public EfCoreBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Adds a single entity to the DbContext
    /// </summary>
    /// <param name="entity">The DbContext Entity to add</param>
    /// <typeparam name="TEntity">The Entities Type</typeparam>
    /// <returns></returns>
    public TBuilder With<TEntity>(TEntity entity) where TEntity : class
    {
        Enqueue(serviceProvider =>
        {   
            var dbContext = serviceProvider.GetRequiredService<TDbContext>();

            if (entity == null) return;
            dbContext.Add(entity);
            dbContext.SaveChanges();
        });

        return (TBuilder) this;
    }

    /// <summary>
    /// Adds a collection of entities to the DbContext
    /// </summary>
    /// <param name="entities">The DbContext Entities to add</param>
    /// <typeparam name="TEntity">The Entity Type</typeparam>
    /// <returns></returns>
    public TBuilder WithRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        Enqueue(serviceProvider =>
        {
            var dbContext = serviceProvider.GetRequiredService<TDbContext>();

            dbContext.AddRange(entities);
            dbContext.SaveChanges();
        });

        return (TBuilder) this;
    }


    /// <summary>
    /// Merges a single entity with and existing entity in the DbContext.
    /// If an existing entity is not found, it will be added instead.
    /// Values are merged with an existing entity, ignoring default values.
    /// </summary>
    /// <param name="entity">The entity that will have it's values merged into an existing entity</param>
    /// <typeparam name="TEntity">The Entities Type</typeparam>
    /// <returns></returns>
    public TBuilder Merge<TEntity>(TEntity entity) where TEntity : class, new()
    {
        Enqueue(serviceProvider =>
        {
            var dbContext = serviceProvider.GetRequiredService<TDbContext>();
            var key = GetKey(entity, dbContext);
            var existingEntity = dbContext.Find<TEntity>(key);

            if (existingEntity == null)
            {
                //If we can't find the entity, we will add it instead.
                With(entity);
                return;
            }

            //Otherwise we will update the existing entity
            var mergedEntity = Merger.CloneAndMerge(existingEntity, entity);
            if (mergedEntity == null) return;

            dbContext.Entry(existingEntity).CurrentValues.SetValues(mergedEntity);
            dbContext.SaveChanges();
        });

        return (TBuilder)this;
    }

    private object[] GetKey<T>(T entity, TDbContext dbContext)
    {
        if(entity == null) throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
        
        var entry = dbContext.Entry(entity);
        object[] keyParts = entry.Metadata.FindPrimaryKey()
            .Properties
            .Select(p => entry.Property(p.Name).CurrentValue)
            .ToArray();
        
        return keyParts;
    }
}