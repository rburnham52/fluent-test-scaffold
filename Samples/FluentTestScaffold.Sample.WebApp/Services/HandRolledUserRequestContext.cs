using System.Security.Claims;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FluentTestScaffold.Sample.WebApp.Services;

/// <summary>
/// Just a simple hand rolled user authentication.
/// </summary>
public class HandRolledUserRequestContext : IUserRequestContext
{
    private readonly TestDbContext _dbContext;
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private User? _user = null;

    public HandRolledUserRequestContext(
        TestDbContext dbContext,
        IAuthService authService,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Will attempt to find a matching user with the supplied email and password.  If a user is found, then we will
    /// create a new Claim Identity with the type "HandRolled" with claims to store the user id (under NameIdentifier),
    /// email and name for the user.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public User? AuthenticateUser(string? email, string? password)
    {
        var user = _authService.AuthenticateUser(
            email ?? throw new ArgumentException($"{nameof(email)} is required"),
            password ?? throw new ArgumentException($"{nameof(password)} is required"));

        if (user == null) return null;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.Now
        };

        _httpContextAccessor.HttpContext?.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties)
            .Wait();

        return user;
    }

    public void SignOut()
    {
        _httpContextAccessor.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        _user = null;
    }

    /// <summary>
    /// Looks for a Claim Identity with the type "HandRolled" and will attempt to load from the database the User with
    /// claim NameIdentifier which should contain the user's user id.
    /// </summary>
    public User? CurrentUser => _user ??= ResolveUserFromHttpContext();

    private User? ResolveUserFromHttpContext()
    {
        var identity = _httpContextAccessor.HttpContext?.User.Identities.FirstOrDefault(id =>
            id.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme);

        if (identity == null) return null;

        if (!Guid.TryParse(
                identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                out var userId))
            return null;

        return _dbContext.Users.Find(userId);
    }
}