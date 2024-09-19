using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Product
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required][DataType(DataType.MultilineText)] public string Description { get; set; }
        [Required] public int CategoryId { get; set; }
        [Required][ForeignKey("CategoryId")] public Category Category { get; set; }
        [Required] public uint SKU { get; set; } // Stock Keeping Unit
        public ulong Views { get; set; } = 0;
        [Required][DataType(DataType.Currency)] public ulong PriceCents { get; set; }
        [Required][Range(0, 99)] public ushort SalePercent { get; set; } = 0;
        public ICollection<Review> Reviews { get; set; }
        [Required] public TimeSpan Warranty { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; }
        
        [FileExtensions(Extensions ="jpg,jpeg,png,gif,mp4,mov,avi,webm")]
        [Required][NotMapped][DataType(DataType.Upload)] public List<IFormFile>? Media { get; set; }
        public ICollection<User> WishlistUsers { get; set; }
    }
}
