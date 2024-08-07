using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Egost.Models;
using Microsoft.AspNetCore.Identity;

namespace Egost.Areas.Identity.Data;

public class User : IdentityUser
{
    [PersonalData] public string Name { get; set; }
    [PersonalData] public DateOnly DOB { get; set; }
    [PersonalData][AllowedValues("Male", "Female")][MaxLength(6)] public string? Gender { get; set; }
    [PersonalData] public ICollection<Address> Addresses { get; set; }
    public ICollection<Product> WishList { get; set; }
    public ICollection<Order> Orders { get; set; }
    public Cart Cart { get; set; }
    [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    public ICollection<EditHistory> EditsHistory { get; set; }
}