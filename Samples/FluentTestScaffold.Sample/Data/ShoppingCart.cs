using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentTestScaffold.Sample.Data;

public class ShoppingCart
{
    [Key] public Guid Id { get; set; }

    [ForeignKey(nameof(User))] public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public List<Item> Inventory { get; set; } = new();
}
