using System;
using System.Linq;
using FluentTestScaffold.EntityFrameworkCore;
using FluentTestScaffold.Sample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentTestScaffold.Tests.CustomBuilder;

public class UserBuilder(IServiceProvider serviceProvider) : EfCoreBuilder<TestDbContext, UserBuilder>(serviceProvider)
{
    public static User Under18User = new User(Guid.NewGuid(), "Bob", "bob@test.com", "SuperSecret123",
        DateTime.Now.AddYears(-12));

    public static User Over18User = new User(Guid.NewGuid(), "Jeff", "Jeff@test.com", "SuperSecret567",
        DateTime.Now.AddYears(-23));

    public UserBuilder WithUnder18User(out Guid userId)
    {
        userId = Under18User.Id;
        With(Under18User)
            .Build();
        return this;
    }
    
    public UserBuilder WithOver18User(out Guid userId)
    {
        userId = Over18User.Id;
        With(Over18User)
            .Build();
        return this;
    }
    
    /// <summary>
    /// Adds a user to the DBContext
    /// </summary>
    public UserBuilder WithUser(User user)
    {
        With(user);
        return this;
    }
    
    /// <summary>
    /// Adds a Shopping cart for the User
    /// </summary>
    public UserBuilder WithShoppingCart(Guid userId)
    {
        With(new ShoppingCart()
        {
            Id = Guid.NewGuid(),
            UserId = userId
        });

        return this;
    }
    
    /// <summary>
    /// Adds an item to the users shopping cart
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public UserBuilder WithItem(Guid userId, Item item)
    {
        Enqueue(s =>
        {
            var dbContext = s.GetRequiredService<TestDbContext>();
            var shoppingCart = dbContext.ShoppingCart
                .Include(s => s.Inventory)
                .First( x => x.UserId == userId);

            shoppingCart.Inventory.Add(item);
            dbContext.SaveChanges();
        });
        
        return this;
    }
    
}