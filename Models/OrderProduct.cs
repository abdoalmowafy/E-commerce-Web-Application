using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class OrderProduct
    {
        [Key] public int Id { get; set; }
        [Required] public Product Product { get; set; }
        [Required][DataType(DataType.Currency)] public ulong ProductPriceCents { get; set; }
        [Required][Range(0, 99)] public float SalePercent { get; set; }
        [Required] public uint Quantity { get; set; }
        [DataType(DataType.DateTime)] public DateTime? PartiallyOrFullyReturnedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; }
    }
}
