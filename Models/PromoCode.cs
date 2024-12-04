using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class PromoCode
    {
        [Key] public int Id { get; set; }
        [Required] public required string Code { get; set; }
        [Required][DataType(DataType.MultilineText)] public required string Description { get; set; }
        [Required][Range(0, 99)] public int Percent { get; set; }
        [Range(0, long.MaxValue)] public long? MaxSaleCents { get; set; }
        [Required] public bool Active { get; set; } = true;
        [Required] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public ICollection<EditHistory> EditsHistory { get; set; } = [];
    }
}
