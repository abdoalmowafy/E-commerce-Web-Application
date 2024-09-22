using Egost.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class Review
    {
        [Key] public int Id { get; set; }
        [Required] public User Reviewer { get; set; }
        [Required] public int ProductId { get; set; }
        [ForeignKey("ProductId")] public Product Product { get; set; }
        [Required][Range(1,5)] public byte Rating { get; set; }
        [DataType(DataType.MultilineText)] public string Text { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)] public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; } = [];
    }
}
