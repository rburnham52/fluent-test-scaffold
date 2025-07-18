using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Sample.WebApp.Model;
using Microsoft.AspNetCore.Mvc;

namespace FluentTestScaffold.Sample.WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserRequestContext _userRequestContext;

    public AuthenticationController(
        IUserRequestContext userRequest)
    {
        _userRequestContext = userRequest;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest loginRequest)
    {
        var user = _userRequestContext.AuthenticateUser(loginRequest.Email, loginRequest.Password);

        if (user == null) return Unauthorized();

        return Ok();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the existing external cookie
        _userRequestContext.SignOut();

        return Ok();
    }
}