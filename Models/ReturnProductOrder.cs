using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class ReturnProductOrder
    {
        [Key] public int Id { get; set; }
        public User? Transporter { get; set; }
        [Required] public Order Order { get; set; }
        [Required] public OrderProduct OrderProduct { get; set; }
        [Required] public string ReturnReason { get; set; }
        [Required] public uint Quantity { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? ReturnedDateTime { get; set; }
    }
}
