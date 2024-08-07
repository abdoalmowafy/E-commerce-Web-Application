using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
