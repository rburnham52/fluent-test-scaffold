using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Sample.WebApp.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FluentTestScaffold.Sample.WebApp.Controllers;


[ApiController]
[Authorize]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRequestContext _userRequestContext;

    public UserController(
        IUserRequestContext userRequestContext)
    {
        _userRequestContext = userRequestContext;
    }
    
    [HttpGet]
    public ActionResult<UserDetails> Get()
    {
        var user = _userRequestContext.CurrentUser;

        if (user == null) return Unauthorized();

        return Ok(new UserDetails(
            user.Id,
            user.Name,
            user.Email,
            user.DateOfBirth));
    }
}