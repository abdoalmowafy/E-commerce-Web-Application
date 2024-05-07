using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class EditHistory
    {
        [Key] public int Id { get; set; }
        [Required] public User Editor { get; set; }
        [Required] public string Field { get; set; }
        [Required] public string OldData { get; set; }
        [Required] public string NewData { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime EditDateTime { get; set; } = DateTime.Now;
    }
}
