using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Egost.Models;
using System.Collections.ObjectModel;

public class AddressesModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public AddressesModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public ICollection<Address> Addresses { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Addresses = user.Addresses ?? [];
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var user = await _userManager.GetUserAsync(User);

        var address = user?.Addresses.FirstOrDefault(a => a.Id == id);
        if (address != null)
        {
            user.Addresses.Remove(address);
            await _userManager.UpdateAsync(user);
        }

        return RedirectToPage();
    }
}
