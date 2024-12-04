using Egost.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [Key] public int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required][DataType(DataType.MultilineText)] public required string Description { get; set; }
    [Required] public int CategoryId { get; set; }
    [Required][ForeignKey("CategoryId")] public Category Category { get; set; }

    [Required][Range(0, int.MaxValue)] public int SKU { get; set; }

    public long Views { get; set; } = 0;

    [Required][Range(0, long.MaxValue)] public long PriceCents { get; set; }

    [Required][Range(0, 99)] public int SalePercent { get; set; } = 0;

    public ICollection<Review> Reviews { get; set; } = [];

    [Required] public TimeSpan Warranty { get; set; }
    [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
    public ICollection<EditHistory> EditsHistory { get; set; } = [];

    [FileExtensions(Extensions = "jpg,jpeg,png,gif,mp4,mov,avi,webm")]
    [Required][NotMapped][DataType(DataType.Upload)] public List<IFormFile>? Media { get; set; }
}
