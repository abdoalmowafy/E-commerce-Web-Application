using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Egost.Data;
using Microsoft.EntityFrameworkCore;

public class AddressesModel(EgostContext db) : PageModel
{
    private readonly EgostContext _db = db;

    public ICollection<Address> Addresses { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _db.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
        Addresses = user.Addresses ?? [];
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var user = await _db.Users.Include(u => u.Addresses).FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

        var address = user!.Addresses.FirstOrDefault(a => a.Id == id);
        if (address == null) return NotFound();

        var activeOrderAddress = _db.Orders.Any(o => o.Address == address && o.DeletedDateTime != null && o.DeliveryDateTime != null);
        if (activeOrderAddress)
        {
            TempData["fail"] = "Address has active Order!";
            return RedirectToPage();
        }

        var activeReturnAddress = _db.ReturnProductOrders.Any(rpo => rpo.Address == address && rpo.DeletedDateTime != null && rpo.ReturnedDateTime != null);
        if (activeReturnAddress) 
        {
            TempData["fail"] = "Address has active Return Product Order!";
            return RedirectToPage();
        }

        user.Addresses.Remove(address);
        _db.Addresses.Remove(address);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}
