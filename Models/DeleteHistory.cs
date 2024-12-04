using Egost.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Egost.Models
{
    public class DeleteHistory
    {
        [Key] public int Id { get; set; }
        [Required] public User? Deleter { get; set; }
        [Required] public required string DeletedModelName { get; set; }
        [Required] public int DeletedId { get; set; }
        [Required][DataType(DataType.DateTime)] public DateTime DeleteDateTime { get; set; } = DateTime.Now;
    }
}
