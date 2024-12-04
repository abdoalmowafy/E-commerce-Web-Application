using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Address
    {
        [Key] public int Id { get; set; }
        [Required] public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        [Required] public required string City { get; set; }
        [Required] public required string Country { get; set; }
        [Required][DataType(DataType.PostalCode)] public required string PostalCode { get; set; }
        [Required][DataType(DataType.PhoneNumber)] public required string Telephone { get; set; }
        [Required] public bool StoreAddress { get; set; } = false;
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; } = [];
        
        [NotMapped] public static Address Base
        {
            get
            {
                return new Address
                {
                    Id = 1,
                    AddressLine1 = "Base",
                    City = "Base",
                    PostalCode = "Base",
                    Country = "Base",
                    Telephone = "Base",
                    StoreAddress = true,
                    CreatedDateTime = new(2024, 1, 1)
                };
            }
        }
    }
}
