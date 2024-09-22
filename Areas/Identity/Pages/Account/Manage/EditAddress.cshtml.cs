using Egost.Areas.Identity.Data;
using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Versioning;
using System.Threading.Tasks;

public class EditAddressModel(EgostContext db) : PageModel
{
    private readonly EgostContext _db = db;


    [BindProperty]
    public Address Address { get; set; }

    public IActionResult OnGet(int id)
    {
        var user = _db.Users.Include(u => u.Addresses).FirstOrDefault(u => u.UserName == User.Identity!.Name);
        Address = user.Addresses.FirstOrDefault(a => a.Id == id);

        if (Address == null)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        var user = _db.Users.Include(u => u.Addresses).FirstOrDefault(u => u.UserName == User.Identity!.Name);
        if (!user.Addresses.Any(a => a.Id == Address.Id))
        {
            return NotFound();
        }

        Address.StoreAddress = false;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var address = _db.Addresses.Find(Address.Id);
        address.Country = Address.Country;
        address.City = Address.City;
        address.AddressLine1 = Address.AddressLine1;
        address.AddressLine2 = Address.AddressLine2;
        address.PostalCode = Address.PostalCode;
        address.Telephone = Address.Telephone;
        address.StoreAddress = Address.StoreAddress;

        _db.Addresses.Update(address);
        _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));
        return RedirectToPage("Addresses");
    }
}
