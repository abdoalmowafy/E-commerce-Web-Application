using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class Cart
    {
        [Key] public int Id { get; set; }
        [Required] public ICollection<CartProduct> CartProducts { get; set; } = [];
        public PromoCode? PromoCode { get; set; }
    }
}
