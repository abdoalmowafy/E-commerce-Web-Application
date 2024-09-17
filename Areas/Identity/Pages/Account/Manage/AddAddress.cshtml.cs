using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

public class AddAddressModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public AddAddressModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

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

        user.Addresses.Add(Address);
        await _userManager.UpdateAsync(user);

        return RedirectToPage("Addresses");
    }
}
