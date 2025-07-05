using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Sample.WebApp.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluentTestScaffold.Sample.WebApp.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly ShoppingCartService _shoppingCartService;

    public ShoppingCartController(
        ShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpPost]
    public void AddItem(
        AddItemToShoppingCartRequest addItemToShoppingCartRequest)
    {
        _shoppingCartService.AddItemToCart(addItemToShoppingCartRequest.ItemId);
    }
}