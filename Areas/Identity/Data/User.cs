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
    [PersonalData] public Gender? Gender { get; set; }
    [PersonalData] public ICollection<Address> Addresses { get; set; }
    public ICollection<Product> WishList { get; set; }
    public ICollection<Order> Orders { get; set; }
    public ICollection<ReturnProductOrder> ReturnProductOrders { get; set; }
    public Cart Cart { get; set; }
    [Required][DataType(DataType.DateTime)] public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    public ICollection<EditHistory> EditsHistory { get; set; } = [];
}

public enum Gender
{
    Male,
    Female
}