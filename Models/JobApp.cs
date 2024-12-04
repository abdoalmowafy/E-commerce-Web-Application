using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Egost.Models
{
    public class JobApp
    {
        [Key] public int Id { get; set; }
        [Required] public required string Postion { get; set; }
        [Required] public required User User { get; set; }
        [Required][NotMapped][DataType(DataType.Upload)] public IFormFile CV { get; set; }
    }
}
