using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Address
    {
        [Key] public int Id { get; set; }
        [Required] public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required] public string City { get; set; }
        [Required][DataType(DataType.PostalCode)] public string PostalCode { get; set; }
        [Required] public string Country { get; set; }
        [Required][DataType(DataType.PhoneNumber)] public string Telephone { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; }
        
        [NotMapped] public static Address Base
        {
            get
            {
                return new Address
                {
                    AddressLine1 = "Base",
                    City = "Base",
                    PostalCode = "Base",
                    Country = "Base",
                    Telephone = "Base"
                };
            }
        }
    }
}
