using Egost.Areas.Identity.Data;
using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Egost.Controllers
{
    public class StoreController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;
        private IEnumerable<string> GetCategoriesNames() => _db.Categories.Select(c => c.Name);


        public IActionResult Home()
        {
            IEnumerable<OrderProduct> OrderedProducts = _db.OrderProducts.Include(op => op.Product);
            IEnumerable<Product> NonDeletedAvailableProducts = _db.Products
                .Include(p => p.Category).Where(p => p.DeletedDateTime == null && p.SKU > 0);
            var freq = NonDeletedAvailableProducts.ToDictionary(p => p, _ => 0UL);
            foreach (var op in OrderedProducts)
            {
                var product = op.Product;
                if (product.DeletedDateTime == null && product.SKU > 0)
                {
                    freq[product]++;
                }
            }

            IEnumerable<Product> orderedByMostOrderes = NonDeletedAvailableProducts.OrderByDescending(p => freq[p]);
            IEnumerable<Product> productsOnSale = orderedByMostOrderes.Where(p => p.SalePercent > 0);
            IEnumerable<Product> productsAddedLastWeek = NonDeletedAvailableProducts.OrderByDescending(p => DateTime.Now - p.CreatedDateTime);
            ViewBag.CategoriesNames = GetCategoriesNames();

            return View((orderedByMostOrderes, productsOnSale, productsAddedLastWeek));
        }

        public IActionResult Search(string keyWord, string categoryName = "All", bool includeOutOfStock = false, bool includeDeleted = false, int pageIndex = 1)
        {
            Category category = _db.Categories.FirstOrDefault(c => c.Name == categoryName);
            // Errors
            if (categoryName != "All" && category == null)
            {
                TempData["fail"] = "Invalid Category!";
                return RedirectToAction("Home");
            }

            // Implementation
            var Products = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(keyWord))
            {
                Products = Products.Where(Product => Product.Name.Contains(keyWord) || Product.Description.Contains(keyWord));
            }

            // Filter by category if provided
            if (categoryName != "All")
            {
                Products = Products.Where(p => p.Category.Name == categoryName);
            }

            // Filter by deleted if included
            if (!includeDeleted)
            {
                Products = Products.Where(p => p.DeletedDateTime == null);
            }
            else if (!User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            {
                return Forbid();
            }

            // Filter by in stock
            if (!includeOutOfStock)
            {
                Products = Products.Where(p => p.SKU > 0);
            }

            // Add Search to db
            if (!string.IsNullOrEmpty(keyWord))
            {
                _db.Searches.Add(new()
                {
                    User = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name),
                    Category = category,
                    KeyWord = keyWord
                });
                _db.SaveChanges();
            }

            TempData["Categories"] = GetCategoriesNames();
            TempData["KeyWord"] = keyWord;
            TempData["Category"] = categoryName;
            TempData["includeOutOfStock"] = includeOutOfStock;
            TempData["IncludeDeleted"] = includeDeleted;

            return View(Products.ToPaginatedList(pageIndex, 10));
        }


        [Route("product/{id:int}")]
        public IActionResult FullView(int Id)
        {
            var Product = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Reviewer)
                .FirstOrDefault(p => p.Id == Id);

            if (Product == null) return NotFound();

            // Increase product views
            Product.Views++;
            _db.Products.Update(Product);
            _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

            // product media
            string path = Path.Combine("wwwroot\\Media\\ProductMedia\\", Product.Id.ToString());
            string[] fileNames = Directory.GetFiles(path);
            List<string> fileNamesOnly = fileNames.Select(filePath => Path.GetFileName(filePath)).ToList();
            ViewBag.FileNames = fileNamesOnly;
            ViewBag.inCart = false;
            ViewBag.inWishlist = false;
            ViewBag.reviewable = false;
            if (User.Identity.IsAuthenticated)
            {
                var user = _db.Users
                    .Include(u => u.Cart)
                        .ThenInclude(c => c.CartProducts)
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                    .Include(u => u.WishList)
                    .FirstOrDefault(u => u.UserName == User.Identity.Name);

                var cart = user.Cart;
                var wishList = user.WishList;
                var reviews = Product.Reviews;
                if (user.Cart.CartProducts != null && cart.CartProducts.Any(cp => cp.Product == Product))
                {
                    ViewBag.inCart = true;
                }
                if (user.WishList != null && wishList.Contains(Product))
                {
                    ViewBag.inWishlist = true;
                }
                if (!Product.Reviews.Any(rev => rev.Reviewer == user && rev.DeletedDateTime == null) 
                    && user.Orders.SelectMany(o => o.OrderProducts).Any(op => op.Product == Product))
                {
                    ViewBag.reviewable = true;
                }
            }
            return View(Product);
        }

        [Authorize]
        public IActionResult IndexWishlist(int pageIndex = 1)
        {
            var user = _db.Users.Include(u => u.WishList).ThenInclude(p => p.Category).FirstOrDefault(u => u.UserName == User.Identity!.Name);

            return View(user.WishList.ToPaginatedList(pageIndex, 10));
        }

        [Authorize]
        public IActionResult ModifyWishlist(int ProductId)
        {
            var user = _db.Users.Include(u => u.WishList).FirstOrDefault(u => u.UserName == User.Identity.Name);
            var product = _db.Products.Find(ProductId);

            if (product == null) return NotFound();

            if (!user.WishList.Remove(product))
            {
                user.WishList.Add(product);
            }
            _db.Users.Update(user);
            _db.SaveChanges();

            return RedirectToAction(nameof(FullView), new { Id = ProductId });
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, byte rating, string text)
        {
            var product = _db.Products.Find(productId);

            if (product == null) return NotFound();

            var user = _db.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (product.Reviews.Any(r => r.Reviewer == user && r.DeletedDateTime == null))
            {
                TempData["info"] = "You can only post one review for the same product!";
            }
            else if (!user.Orders.SelectMany(o => o.OrderProducts).Any(op => op.Product.Id == productId))
            {
                TempData["info"] = "Can't review a product you didn't buy!";
            }
            else
            {
                Review review = new()
                {
                    Product = product,
                    Reviewer = user,
                    Rating = rating,
                    Text = text,
                };
                _db.Reviews.Add(review);
                product.Reviews.Add(review);
                _db.Products.Update(product);
                _db.SaveChanges();
            }

            return RedirectToAction(nameof(FullView), new { Id = productId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditReview(int reviewId, byte rating, string text)
        {
            var review = _db.Reviews
               .Include(r => r.Reviewer)
               .FirstOrDefault(r => r.Id == reviewId);

            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (review == null || review.DeletedDateTime.HasValue) return NotFound();

            if (review.Reviewer != user && !User.IsInRole("Admin") && !User.IsInRole("Moderator")) return Forbid();
            
            if(review.Rating != rating) review.Rating = rating;
            if(review.Text != text) review.Text = text;
            _db.Reviews.Update(review);
            _db.SaveChanges(user);

            return RedirectToAction(nameof(FullView), new { Id = review.ProductId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReview(int reviewId)
        {
            var review = _db.Reviews
               .Include(r => r.Reviewer)
               .FirstOrDefault(r => r.Id == reviewId);

            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (review == null || review.DeletedDateTime.HasValue) return NotFound();

            if (review.Reviewer != user && !User.IsInRole("Admin") && !User.IsInRole("Moderator")) return Forbid();

            review.DeletedDateTime = DateTime.Now;
            _db.Reviews.Update(review);
            _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

            return RedirectToAction(nameof(FullView), new { Id = review.ProductId });
        }
    }
}