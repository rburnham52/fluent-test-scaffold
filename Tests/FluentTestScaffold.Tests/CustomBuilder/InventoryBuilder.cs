using System;
using System.Collections.Generic;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;

namespace FluentTestScaffold.Tests.CustomBuilder;

public class InventoryBuilder : EfCoreBuilder<TestDbContext, InventoryBuilder>
{
    public InventoryBuilder(IServiceProvider serviceProvider) : base(serviceProvider) { }

    /// <summary>
    /// Adds a set of sample Items to the DBContext
    /// </summary>
    public InventoryBuilder WithDefaultCatalogue()
    {
        WithRange(new List<Item>()
        {
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Minions, Price = 21},
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Avengers, Price = 24},
            new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.DeadPool, Price = 14, AgeRestriction = 15}
        });

        return this;
    }
}