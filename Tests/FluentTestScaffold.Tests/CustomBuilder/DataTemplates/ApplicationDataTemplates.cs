using System;
using System.Collections.Generic;
using FluentTestScaffold.Core;
using FluentTestScaffold.Sample;
using FluentTestScaffold.Sample.Data;

namespace FluentTestScaffold.Tests.CustomBuilder.DataTemplates;


public class ApplicationDataTemplates
{
    [DataTemplate]
    public TestScaffold DefaultCatalogueAndUsers(TestScaffold testScaffold)
    {
        testScaffold.UsingBuilder<UserBuilder>()
            //Setup standard users
            .WithOver18User(out var over18UserId)
            .SetTestContext("Over18UserId", over18UserId)
            .WithUnder18User(out var under18UserId)
            .SetTestContext("Under18UserId", under18UserId)
            //Setup User Shopping Carts
            .WithShoppingCart(under18UserId)
            .WithShoppingCart(over18UserId)
            .Build()
            .UsingBuilder<InventoryBuilder>()
            //Setup Inventory
            .WithRange(new List<Item>()
            {
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Minions, Price = 21},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.Avengers, Price = 24},
                new() {Id = Guid.NewGuid(), Title = Defaults.CatalogueItems.DeadPool, Price = 14, AgeRestriction = 15}
            }).Build();
        
        return testScaffold;
    }
}
