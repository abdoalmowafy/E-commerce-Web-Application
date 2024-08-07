using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class ReturnProduct
    {
        [Key] public int Id { get; set; }
        [Required] public OrderProduct OrderProduct { get; set; }
        [Required] public uint Quantity { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
    }
}
