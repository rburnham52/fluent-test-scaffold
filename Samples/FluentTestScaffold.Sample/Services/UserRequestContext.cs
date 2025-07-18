using FluentTestScaffold.Sample.Data;

namespace FluentTestScaffold.Sample.Services;

//Mock Authenticated User Context
public class UserRequestContext : IUserRequestContext
{
    private readonly IAuthService _authService;
    private User? _user;


    public User? CurrentUser => _user;

    public UserRequestContext(IAuthService authService)
    {
        _authService = authService;
    }

    public void SignOut()
    {
        _user = null;
    }

    public User? AuthenticateUser(string? email, string? password)
    {
        if (email != null && password != null)
            _user = _authService.AuthenticateUser(email, password);

        return _user;
    }
}

public interface IUserRequestContext
{
    User? AuthenticateUser(string? email, string? password);

    void SignOut();

    User? CurrentUser { get; }
}