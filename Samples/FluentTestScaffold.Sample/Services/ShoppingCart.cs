using FluentTestScaffold.Sample.Data;

namespace FluentTestScaffold.Sample.Services;

/// <summary>
/// Example of a service that benefits from a Component Integration Test
/// </summary>
public class ShoppingCartService
{
    private readonly TestDbContext _dbContext;
    private readonly IUserRequestContext _userRequestContext;

    public ShoppingCartService(TestDbContext dbContext, IUserRequestContext userRequestContext)
    {
        _dbContext = dbContext;
        _userRequestContext = userRequestContext;
    }

    public void AddItemToCart(Guid itemId)
    {
        var user = _userRequestContext.CurrentUser;
        if (user == null) return;

        var item = _dbContext.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) throw new InvalidOperationException("Could not find Item");
        VerifyAgeRestriction(user, item);
        var shoppingCart = _dbContext.ShoppingCart.FirstOrDefault(c => c.UserId == user.Id);
        if (shoppingCart is null)
        {
            shoppingCart = new ShoppingCart()
            {
                UserId = user.Id
            };

            _dbContext.ShoppingCart.Add(shoppingCart);
            _dbContext.SaveChanges();
        }

        shoppingCart.Inventory.Add(item);

        _dbContext.SaveChanges();
    }

    private void VerifyAgeRestriction(User user, Item item)
    {
        var userAge = Years(user.DateOfBirth, DateTime.Now);
        if (userAge < item.AgeRestriction)
            throw new InvalidOperationException($"You must be over {item.AgeRestriction} to add this item");
    }

    int Years(DateTime start, DateTime end)
    {
        return (end.Year - start.Year - 1) +
               (((end.Month > start.Month) ||
                 ((end.Month == start.Month) && (end.Day >= start.Day))) ? 1 : 0);
    }
}
