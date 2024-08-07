using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class Search
    {
        [Key] public int Id { get; set; }
        public User? User { get; set; }
        [Required] public string KeyWord { get; set; }
        public Category? Category { get; set; }
    }
}
