using System.ComponentModel.DataAnnotations;

namespace FluentTestScaffold.Sample.Data;

public class User
{
    public User(Guid id, string name, string email, string password, DateTime dateOfBirth)
    {
        Id = id;
        Name = name;
        Email = email;
        Password = password;
        DateOfBirth = dateOfBirth;
    }

    [Key] public Guid Id { get; set; }

    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime DateOfBirth { get; set; }

    public ShoppingCart ShoppingCart { get; set; }
}