using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class CartProduct
    {
        [Key] public int Id { get; set; }
        [Required] public Product Product { get; set; }
        [Required] public uint Quantity { get; set; }
    }
}
