using Egost.Areas.Identity.Data;
using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Egost.Controllers
{
    public class StoreController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;
        private IEnumerable<string> GetCategoriesNames() => _db.Categories.Select(c => c.Name);


        public IActionResult Home()
        {
            // Most Ordered , Sale , Categories , ....
            IEnumerable<Product> NonDeletedProducts = _db.Products
                .Include(p => p.Category).Where(p => p.DeletedDateTime == null);
            return View(NonDeletedProducts);
        }

        public IActionResult Search(string keyWord, string categoryName = "All", bool includeOutOfStock = false, bool includeDeleted = false)
        {
            Category category = _db.Categories.FirstOrDefault(c => c.Name == categoryName);
            // Errors
            if (categoryName != "All" && category == null)
            {
                TempData["fail"] = "Invalid Category!";
                return RedirectToAction("Home");
            }

            // Implementation
            IEnumerable<Product> Products = _db.Products.Include(p => p.Category);

            if (!string.IsNullOrEmpty(keyWord))
            {
                Products = Products.Where(Product => Product.Name.Contains(keyWord, StringComparison.OrdinalIgnoreCase) ||
                                            Product.Description.Contains(keyWord, StringComparison.OrdinalIgnoreCase));
                if (Products.Any())
                {
                    TempData["info"] = $"{Products.Count()} results For \"{keyWord}\"";
                }
                else
                {
                    TempData["fail"] = $"No Products Found For \"{keyWord}\"!";
                }
            }

            // Filter by category if provided
            if (categoryName != "All")
            {
                Products = Products.Where(p => p.Category.Name == categoryName);
            }

            // Filter by deleted if included
            if (includeDeleted)
            {
                if (!User.IsInRole("Admin") && !User.IsInRole("Moderator"))
                    return Forbid(); // If user is not authorized, return 403 Forbidden
            }
            else
            {
                Products = Products.Where(p => p.DeletedDateTime == null);
            }

            // Filter by in stock
            if (!includeOutOfStock)
            {
                Products = Products.Where(p => p.SKU > 0);
            }

            // Add Search to db
            _db.Searches.Add(new()
            {
                User = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name),
                Category = category,
                KeyWord = keyWord
            });
            _db.SaveChanges();

            TempData["Categories"] = GetCategoriesNames();
            TempData["KeyWord"] = keyWord;
            TempData["Category"] = categoryName;
            TempData["includeOutOfStock"] = includeOutOfStock;
            TempData["IncludeDeleted"] = includeDeleted;

            return View(Products);
        }


        [Route("{id}")]
        public IActionResult FullView(int Id)
        {
            var Product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefault(p => p.Id == Id);

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
                var user = _db.Users
                    .Include(u => u.Cart)
                        .ThenInclude(c => c.CartProducts)
                    .Include(u => u.WishList)
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);

                var cart = user.Cart;
                var wishList = user.WishList;
                if (cart.CartProducts != null && cart.CartProducts.Any(cp => cp.Product == Product)) {
                    ViewBag.inCart = true;
                }
                if (wishList != null && wishList.Contains(Product))
                {
                    ViewBag.inWishlist = true;
                }
            }
            return View(Product);
        }


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
                user.WishList = [product];
            }
            if (!user.WishList.Remove(product))
            {
                user.WishList.Add(product);
            }
            _db.Users.Update(user);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = ProductId });
        }
        
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int ProductId, byte Rate, string Text)
        {
            var product = _db.Products.Find(ProductId);

            if (product == null)
            {
                TempData["Danger"] = "Invalid product!";
                return RedirectToAction("Index");
            }

            var user = _db.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (product.Reviews.Any(r => r.Reviewer == user)) 
            {
                TempData["info"] = "Can't post more than a review for the same product!";
            }
            else if (!user.Orders.SelectMany(o => o.OrderProducts).Any(op => op.Product.Id == ProductId))
            {
                TempData["info"] = "Buy product first!";
            }
            else
            {
                Review review = new()
                {
                    Reviewer = user,
                    Rate = Rate,
                    Text = Text,
                };
                _db.Reviews.Add(review);
                product.Reviews.Add(review);
                _db.Products.Update(product);
                _db.SaveChanges();
            }

            return RedirectToAction("Index", new { id = ProductId });
        }
    }
}
