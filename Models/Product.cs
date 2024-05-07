using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Product
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required][DataType(DataType.MultilineText)] public string Description { get; set; }
        [AllowedValues("Sports, Instruments & Accessories", "Toys, Games, Video Games & Accessories",
            "Arts, Crafts & Sewing", "Clothing, Shoes & Jewelry", "Beauty & Personal Care",
            "Books", "Electronics & Accessories", "Software", "Grocery & Gourmet Food",
            "Home Furniture & Accessories", "Luggage & Travel Gear", "Pet Supplies")]
        [Required] public string Category { get; set; }
        [Required] public uint SKU { get; set; } // Stock Keeping Unit
        [Required][DataType(DataType.Currency)] public ulong PriceCents { get; set; }
        [Required][Range(0, 99)] public float SalePercent { get; set; } = 0;
        public ICollection<Review> Reviews { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; }
        [Required][NotMapped][DataType(DataType.Upload)][FileExtensions(Extensions ="jpg,jpeg,png,gif,mp4,mov,avi,webm")] public List<IFormFile>? Media { get; set; }


        [NotMapped] public static string[] Categories {
            get {
                return ["Sports, Instruments & Accessories",
            "Toys, Games, Video Games & Accessories",
            "Arts, Crafts & Sewing",
            "Clothing, Shoes & Jewelry",
            "Beauty & Personal Care",
            "Books",
            "Electronics & Accessories",
            "Software",
            "Grocery & Gourmet Food",
            "Home Furniture & Accessories",
            "Luggage & Travel Gear",
            "Pet Supplies"];
            }
        }
    }
}

