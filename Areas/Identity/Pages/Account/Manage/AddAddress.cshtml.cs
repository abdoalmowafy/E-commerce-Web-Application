using Egost.Areas.Identity.Data;
using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

public class AddAddressModel(UserManager<User> userManager, EgostContext db) : PageModel
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly EgostContext _db = db;


    [BindProperty]
    public Address Address { get; set; }

    public void OnGet()
    {
        
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Address.StoreAddress = false;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _db.Addresses.AddAsync(Address);
        await _db.SaveChangesAsync();
        if (user.Addresses.IsNullOrEmpty()) user.Addresses = [];
        user.Addresses.Add(Address);
        await _userManager.UpdateAsync(user);

        return RedirectToPage("Addresses");
    }
}
