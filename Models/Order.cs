using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class Order
    {
        [Key] public int Id { get; set; }
        [Required] public User? User { get; set; }
        [Required] public ICollection<OrderProduct> OrderProducts { get; set; }
        public PromoCode PromoCode { get; set; }
        [Required][DataType(DataType.Currency)] public ulong TotalCents { get; set; }
        [Required] public string Currency { get; set; } = "EGP";
        [Required][AllowedValues("COD", "CreditCard", "MobileWallet")] public string PaymentMethod { get; set; }
        [Required] public bool DeliveryNeeded { get; set; } = false;
        [Required] public Address Address { get; set; } = Address.Base;
        public User Transporter { get; set; }
        [DataType(DataType.DateTime)] public DateTime? DeliveryDateTime { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; }
    }
}
