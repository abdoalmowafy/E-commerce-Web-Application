using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class ReturnOrder
    {
        [Key] public int Id { get; set; }
        [Required] public Order Order { get; set; }
        [Required] public ICollection<ReturnProduct> ReturnProducts { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
    }
}
