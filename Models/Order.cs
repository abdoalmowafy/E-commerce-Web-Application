using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Order
    {
        [Key] public int Id { get; set; }
        [Required] public string UserId { get; set; }
        [ForeignKey("UserId")] public User User { get; set; }
        public string? TransporterId { get; set; }
        [ForeignKey("TransporterId")] public User? Transporter { get; set; }
        [Required] public ICollection<OrderProduct> OrderProducts { get; set; }
        public PromoCode? PromoCode { get; set; }
        [Required][DataType(DataType.Currency)] public ulong TotalCents { get; set; }
        [Required] public string Currency { get; set; } = "EGP";
        [Required][AllowedValues("COD", "CreditCard", "MobileWallet")] public string PaymentMethod { get; set; }
        [Required] public bool DeliveryNeeded { get; set; } = false;
        [Required] public bool Processed { get; set; }
        public int? PaymobOrderId { get; set; }
        [Required] public Address Address { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeliveryDateTime { get; set; }
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
    }
}
