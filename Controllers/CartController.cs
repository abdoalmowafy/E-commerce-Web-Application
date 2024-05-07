using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Egost.Controllers
{
    [Authorize]
    public class CartController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;

        public IActionResult Index()
        {
            var cart = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name).Cart;
            var promo = cart.PromoCode;

            ViewBag.PromoCode = cart.PromoCode;

            return View(cart.CartProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ModifyProducts(int ProductId, uint count = 1)
        {
            var product = _db.Products.Find(ProductId);
            
            // Redirect to Product Index if ProductId is not valid
            if (product == null)
            {
                return RedirectToAction("Index", "Store");
            }
            
            var cart = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)!.Cart;
            var cartProduct = cart.CartProducts.FirstOrDefault(x => x.Product == product);

            // Get the Dictionary of Products in the cart and its count
            if (count > 0)
            {
                if (cartProduct == null)
                {
                    // Add Product to Cart
                    cartProduct = new CartProduct
                    {
                        Product = product,
                        Quantity = count
                    };
                    _db.CartProducts.Add(cartProduct);
                    cart.CartProducts.Add(cartProduct);
                    _db.Carts.Update(cart);
                }
                else
                {
                    // Edit Quantity
                    cartProduct.Quantity = count;
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
        public IActionResult ApplyPromoCode(string PromoCode)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var cart = user.Cart;
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
            return RedirectToAction("Index");
        }
    }
}
