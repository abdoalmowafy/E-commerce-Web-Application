using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Egost.Controllers
{
    [Authorize]
    public class CartController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;

        public IActionResult Index()
        {
            var user = _db.Users
                .Include(u => u.Addresses)
                .Include(u => u.Cart)
                    .ThenInclude(c => c.CartProducts)
                        .ThenInclude(cp => cp.Product)
                .Include(u => u.Cart)
                    .ThenInclude(c => c.PromoCode)
                .FirstOrDefault(u => u.UserName == User.Identity!.Name);
            var cart = user!.Cart;
            var addresses = user.Addresses;

            var invalidCartProducts = cart.CartProducts.Where(cp => cp.Product.DeletedDateTime.HasValue || cp.Product.SKU < 1 || cp.Quantity > cp.Product.SKU);
            if (invalidCartProducts.Any()) 
            {
                foreach (var cartProduct in invalidCartProducts)
                {
                    cart.CartProducts.Remove(cartProduct);
                    _db.CartProducts.Remove(cartProduct);
                }
                _db.Carts.Update(cart);
                _db.SaveChanges();
            }

            if (cart.PromoCode != null && (!cart.PromoCode.Active || cart.PromoCode.DeletedDateTime.HasValue))
            {
                cart.PromoCode = null;
                _db.Carts.Update(cart);
                _db.SaveChanges();
            }
            
            ViewBag.PromoCode = cart.PromoCode;
            ViewBag.StoreAddresses = _db.Addresses.Where(ad => ad.StoreAddress);
            ViewBag.UserAddresses = addresses;

            return View(cart.CartProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifyProducts(int ProductId, uint Count = 1)
        {
            var product = _db.Products.Find(ProductId);
            
            // Redirect to Product Index if ProductId is not valid
            if (product == null || product.DeletedDateTime.HasValue || product.SKU < 1 || Count > product.SKU)
            {
                return RedirectToAction("Home", "Store");
            }
            
            var cart = _db.Users
                .Include (u => u.Cart)
                    .ThenInclude (c => c.CartProducts)
                .FirstOrDefault(u => u.UserName == User.Identity!.Name)!.Cart;
            
            var cartProduct = cart.CartProducts.FirstOrDefault(x => x.Product == product);

            // Get the Dictionary of Products in the cart and its count
            if (Count > 0)
            {
                if (cartProduct == null)
                {
                    // Add Product to Cart
                    cartProduct = new CartProduct
                    {
                        Product = product,
                        Quantity = Count
                    };
                    _db.CartProducts.Add(cartProduct);
                    cart.CartProducts.Add(cartProduct);
                    _db.Carts.Update(cart);
                }
                else
                {
                    // Edit Quantity
                    cartProduct.Quantity = Count;
                    _db.CartProducts.Update(cartProduct);
                }
            }
            else 
            { 
                if (cartProduct != null)
                {
                    // Remove Product form Cart
                    cart.CartProducts.Remove(cartProduct);
                    _db.CartProducts.Remove(cartProduct);
                    _db.Carts.Update(cart);
                }
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyPromoCode(string? PromoCode)
        {
            var cart = _db.Users
                .Include(u => u.Cart)
                    .ThenInclude(c => c.PromoCode)
                .FirstOrDefault(u => u.UserName == User.Identity!.Name)!.Cart;

            if (PromoCode.IsNullOrEmpty())
            {
                // Remove promocode
                cart.PromoCode = null;
                _db.Carts.Update(cart);
                _db.SaveChanges();
            }
            else
            {
                // Add promocode
                var promo = _db.PromoCodes.FirstOrDefault(x => x.Code == PromoCode);
                if (promo == null)
                {
                    TempData["fail"] = "Invalid Promocode!";
                }
                else
                {
                    cart.PromoCode = promo;
                    _db.Carts.Update(cart);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
