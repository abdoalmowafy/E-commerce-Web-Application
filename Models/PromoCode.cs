using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class PromoCode
    {
        [Key] public int Id { get; set; }
        [Required] public string Code { get; set; }
        [Required][DataType(DataType.MultilineText)] public string Description { get; set; }
        [Required][Range(0, 99)] public ushort Percent { get; set; }
        public ulong? MaxSaleCents { get; set; }
        [Required] public bool Active { get; set; } = true;
        [Required] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public DateTime? DeletedDateTime { get; set; }
        public ICollection<EditHistory> EditsHistory { get; set; } = [];
    }
}
