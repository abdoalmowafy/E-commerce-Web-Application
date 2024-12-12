using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class ReturnProductOrder
    {
        [Key] public int Id { get; set; }
        public User? Transporter { get; set; }
        [Required] public required Address Address { get; set; }
        [Required] public required Order Order { get; set; }
        [Required] public required OrderProduct OrderProduct { get; set; }
        [Required] public required string ReturnReason { get; set; }
        [Required][Range(0, int.MaxValue)] public int Quantity { get; set; }
        [Required] public ReturnStatus Status { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? ReturnedDateTime { get; set; }
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
    }

    public enum ReturnStatus
    {
        Processing,
        OnTheWay,
        Returned,
        Deleted
    }
}
