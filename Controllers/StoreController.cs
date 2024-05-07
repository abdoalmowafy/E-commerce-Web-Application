using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Egost.Controllers
{
    public class StoreController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;


        public IActionResult Home()
        {
            // Most Ordered , Sale , Categories , ....
            IEnumerable<Product> NonDeletedProducts = _db.Products.Where(p => p.DeletedDateTime == null);
            return View(NonDeletedProducts);
        }

        public IActionResult Search(string search, string category = "All", bool filterOutOfStock = false, bool includeDeleted = false)
        {
            // Errors
            if (category != "All" && !Product.Categories.Contains(category))
            {
                TempData["fail"] = "Wrong Category!";
                return RedirectToAction("Index");
            }

            // Implementation
            IEnumerable<Product> Products = _db.Products;

            if (!string.IsNullOrEmpty(search))
            {
                Products = Products.Where(Product => Product.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                            Product.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
                if (Products.Any())
                {
                    TempData["success"] = $"{Products.Count()} results For \"{search}\"";
                }
                else
                {
                    TempData["fail"] = $"No Products Found For \"{search}\"!";
                }
            }

            if (category != "All")
            {
                Products = Products.Where(p => p.Category == category);
            }

            if (includeDeleted)
            {
                if (!User.IsInRole("Admin") && !User.IsInRole("Moderator"))
                    return Forbid(); // If user is not authorized, return 403 Forbidden
            }
            else
            {
                Products = Products.Where(p => p.DeletedDateTime == null);
            }

            if (filterOutOfStock)
            {
                Products = Products.Where(p => p.SKU > 0);
            }

            return View("Index", Products);
        }

        public IActionResult FullView(int? Id)
        {
            var Product = _db.Products.Find(Id);
            if (Product == null)
            {
                TempData["fail"] = "Product not found!";
                return Redirect("/");
            }
            string path = Path.Combine(@"wwwroot\ProductMedia\", Product.Id.ToString());
            string[] fileNames = Directory.GetFiles(path);
            List<string> fileNamesOnly = fileNames.Select(filePath => Path.GetFileName(filePath)).ToList();
            ViewBag.FileNames = fileNamesOnly;
            ViewBag.inCart = false;
            ViewBag.inWishlist = false;
            if (User.Identity.IsAuthenticated)
            {
                var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
                var cart = user.Cart;
                var wishList = user.WishList;
                if(cart.CartProducts.Any(cp => cp.Product == Product)) {
                    ViewBag.inCart = true;
                }
                if (wishList != null && wishList.Contains(Product))
                {
                    ViewBag.inWishlist = true;
                }
            }
            return View(Product);
        }


        [Authorize]
        public IActionResult Wishlist()
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            return View("Index", user.WishList);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifyWishlist(int ProductId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var product = _db.Products.Find(ProductId);

            if (user.WishList.IsNullOrEmpty())
            {
                user.WishList = [ product ];
            }
            if (!user.WishList.Remove(product))
            {
                user.WishList.Add(product);
            }
            _db.Users.Update(user);
            _db.SaveChanges();

            return RedirectToAction("FullView", new { id = ProductId });
        }
    }
}
