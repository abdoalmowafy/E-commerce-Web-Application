using Egost.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Egost.Data;
using Microsoft.EntityFrameworkCore;

namespace Egost.Controllers
{
    public class ManageController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;
        private IEnumerable<string> GetCategoriesNames() => _db.Categories.Select(c => c.Name);



        //Product Managment
        // GET
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult NewProduct()
        {
            ViewBag.CategoriesNames = GetCategoriesNames();
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult NewProduct(Product obj)
        {
            TimeSpan minReturnTime = new (14, 0, 0, 0);
            if (obj.Warranty < minReturnTime) obj.Warranty = minReturnTime;

            if (ModelState.IsValid)
            {
                // Add Product To Database before uploading media To Determine its Id
                _db.Products.Add(obj);
                _db.SaveChanges();

                // Create the directory to save Media Files
                string directoryPath = Path.Combine("wwwroot/Media/ProductMedia/", obj.Id.ToString());
                Directory.CreateDirectory(directoryPath);

                // Save each uploaded file to the desired location
                foreach (var file in obj.Media)
                {
                    string filePath = Path.Combine(directoryPath, file.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                }

                TempData["success"] = "Product Created successfully!";
                return RedirectToAction("Home", "Store");
            }

            ViewBag.CategoriesNames = GetCategoriesNames();
            return View(obj);
        }

        // GET
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult EditProduct(int? Id)
        {
            var OldProduct = _db.Products.Find(Id);

            // Check If Product Exists
            if (OldProduct == null)
            {
                TempData["fail"] = "Product not found!";
                return RedirectToAction("Home", "Store");
            }

            // Get Product Media File Names
            string path = Path.Combine("wwwroot/Media/ProductMedia/", OldProduct.Id.ToString());
            string[] fileNames = Directory.GetFiles(path);
            List<string> fileNamesOnly = fileNames.Select(filePath => Path.GetFileName(filePath)).ToList();
            
            ViewBag.FileNames = fileNamesOnly;
            ViewBag.Categories = GetCategoriesNames();

            return View(OldProduct);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult EditProduct(Product obj)
        {
            TimeSpan minReturnTime = new (14, 0, 0, 0);
            if (obj.Warranty < minReturnTime) obj.Warranty = minReturnTime;

            if (ModelState.IsValid)
            {
                if (obj.Media != null && obj.Media.Count != 0)
                {

                    // Create the directory if it doesn't exist
                    string directoryPath = Path.Combine(@"wwwroot\ProductMedia\", obj.Id.ToString());
                    Directory.CreateDirectory(directoryPath);

                    // Save each uploaded file to the desired location
                    foreach (var file in obj.Media)
                    {
                        string filePath = Path.Combine(directoryPath, file.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                    }
                }

                // Update Product data in Database
                _db.Products.Update(obj);
                _db.SaveChanges();

                TempData["success"] = "Product modified successfully!";
                return RedirectToAction("Home", "Store");
            }

            ViewBag.Categories = GetCategoriesNames();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult DeleteProduct(int Id)
        {
            var product = _db.Products.Find(Id);
            
            if (product == null)
            {
                TempData["fail"] = "Product not found!";
                return RedirectToAction("Home", "Store");
            }

            // Marj product as deleted
            product.DeletedDateTime = DateTime.Now;
            _db.Products.Update(product);
            _db.SaveChanges();

            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Home", "Store");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecoverProduct(int Id)
        {
            var product = _db.Products.Find(Id);

            if (product == null)
            {
                TempData["fail"] = "Product not found!";
                return RedirectToAction("Home", "Store");
            }

            // Mark product as not deleted
            product.DeletedDateTime = null;
            _db.Products.Update(product);
            _db.SaveChanges();

            TempData["success"] = "Product recovered successfully!";
            return RedirectToAction("Home", "Store");
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ChartView()
        {
            var productMetrics = _db.Products.Select(product => new
            {
                product.Id,
                product.Name,
                TotalSales = _db.OrderProducts.Where(op => op.Product.Id == product.Id).Sum(op => (int)op.Quantity),
                Revenue = _db.OrderProducts.Where(op => op.Product.Id == product.Id).Sum(op => op.ProductPriceCents * (1 - op.SalePercent / 100.0) * op.Quantity),
                StockLevel = product.SKU,
                ProductViews = product.Views,
                CartCount = _db.CartProducts.Where(cp => cp.Product.Id == product.Id).Count(),
                WishlistCount = _db.Users.Where(u => u.WishList.Any(p => p.Id == product.Id)).Count(),
                ReturnRate = _db.ReturnProductOrders.Where(rpo => rpo.OrderProduct.Product.Id == product.Id).Count() / (double)(_db.OrderProducts.Where(op => op.Product.Id == product.Id).Count() + 1),
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0
            }).ToList();

            return View(productMetrics);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ProductCharts(int id)
        {
            var product = _db.Products
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            var cartCount = _db.CartProducts
                .Where(cp => cp.Product.Id == id).Count();

            var wishlistCount = _db.Users.Include(u => u.WishList)
                .Where(u => u.WishList.Any(p => p.Id == id)).Count();

            var totalSales = _db.OrderProducts
                .Where(op => op.Product.Id == id)
                .Sum(op => op.Quantity);

            var revenue = _db.OrderProducts
                .Where(op => op.Product.Id == id)
                .Sum(op => op.ProductPriceCents * (1 - (op.SalePercent / 100.0)) * op.Quantity);

            var returnCount = _db.ReturnProductOrders
                .Where(rpo => rpo.OrderProduct.Product.Id == id)
                .Count();

            var Ratings = new ulong[5];
            foreach (var review in product.Reviews) Ratings[review.Rating - 1]++;

            var productMetrics = new
            {
                product.Id,
                product.Name,
                cartCount,
                wishlistCount,
                product.Views,
                product.SKU,
                totalSales,
                revenue,
                returnCount,
                Ratings
            };

            return View(productMetrics);
        }



        // Orders Managment
        [Authorize(Roles = "Admin,Moderator,Transporter")]
        public IActionResult IndexAllOrders(bool undeliveredOnly = false)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name);
            IEnumerable<Order> orders = _db.Orders.Include(o => o.Transporter).Include(o => o.OrderProducts);
            IEnumerable<ReturnProductOrder> returnProductOrders = _db.ReturnProductOrders.Include(rpo => rpo.Transporter);
            if (User.IsInRole("Transporter"))
            {
                orders = orders.Where(u => u.Transporter == user);
                returnProductOrders = returnProductOrders.Where(u => u.Transporter == user);
            }
            if (undeliveredOnly)
            {
                orders = orders.Where(o => o.DeliveryDateTime == null);
                returnProductOrders = returnProductOrders.Where(rpo => rpo.ReturnedDateTime == null);
            }
            
            return View((orders,returnProductOrders));
        }        

        [Authorize(Roles = "Admin,Moderator,Transporter")]
        public IActionResult Delivered(int? OrderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = _db.Orders.Include(o => o.Transporter).FirstOrDefault(o => o.Id == OrderId);
            if (order == null || (User.IsInRole("Transporter") && order.Transporter != user))
            {
                TempData["info"] = "Something went wrong!";
            }
            else
            {
                order.DeliveryDateTime = DateTime.Now;
                _db.Orders.Update(order);
                _db.SaveChanges();
                TempData["success"] = "Delivered Successfully!";
            }
            return RedirectToAction("IndexAllOrders");
        }

        [Authorize(Roles = "Admin,Moderator,Transporter")]
        public IActionResult Returned(int? ReturnProductOrderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var returnProductOrder = _db.ReturnProductOrders
                .Include(rpo => rpo.Transporter).FirstOrDefault(rpo => rpo.Id == ReturnProductOrderId);

            if (returnProductOrder == null || (User.IsInRole("Transporter") && returnProductOrder.Transporter != user))
            {
                TempData["info"] = "Something went wrong!";
            }
            else
            {
                returnProductOrder.ReturnedDateTime = DateTime.Now;
                _db.ReturnProductOrders.Update(returnProductOrder);
                _db.SaveChanges();
                TempData["success"] = "Returned Successfully!";
            }

            return RedirectToAction("IndexAllOrders");
        }




        // Store Addresses Managment
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult IndexStoreAddresses()
        {
            return View(_db.Addresses.Where(adr => adr.StoreAddress));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult NewStoreAddress()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult NewStoreAddress(Address newAddress)
        {
            newAddress.StoreAddress = true;
            if (ModelState.IsValid)
            {
                _db.Addresses.Add(newAddress);
                _db.SaveChanges();
                TempData["success"] = "Address Added successfully!";
                return RedirectToAction(nameof(IndexStoreAddresses));
            }
            return View(newAddress);
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult EditStoreAddress(int storeAddressId)
        {
            var OldStoreAddress = _db.Addresses.Find(storeAddressId);

            // Check If Address Exists
            if (OldStoreAddress == null || !OldStoreAddress.StoreAddress)
            {
                TempData["fail"] = "Address not found!";
                return Redirect("/");
            }

            return View(OldStoreAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult EditStoreAddress(Address storeAddress)
        {
            storeAddress.StoreAddress = true;
            if (ModelState.IsValid)
            {
                _db.Addresses.Update(storeAddress);
                _db.SaveChanges();
                TempData["info"] = "Address Updated Successfully!";
                return RedirectToAction(nameof(IndexStoreAddresses));
            }

            return View(storeAddress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult DeleteStoreAddress(int storeAddressId)
        {
            var storeAddress = _db.Addresses.Find(storeAddressId);

            // Check If Address Exists
            if (storeAddress == null || !storeAddress.StoreAddress)
            {
                TempData["fail"] = "Address not found!";
                return Redirect("/");
            }

            _db.Addresses.Remove(storeAddress);
            _db.SaveChanges();
            return View(storeAddress);
        }


        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult MostSearchInLast(int year = 0, int month = 0, int day = 0, int hour = 0, int minute = 0)
        {
            var searches = _db.Searches;
            if (year != 0 || month != 0 || day != 0 || hour != 0 || minute != 0)
            {
                DateTime untill = new(year, month, day, hour, minute, 0);
            }
            var keyWords = searches.SelectMany(s => s.KeyWord.Split());
            return View(keyWords);
        }

        [Authorize]
        [Route("/Contact/ApplyForJob")]
        public IActionResult SubmitJobApplication() 
        { 
            return View(); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [Route("/Contact/ApplyForJob")]
        public IActionResult SubmitJobApplication(string position, IFormFile CV)
        {
            var user = _db.Users.First(u => u.UserName == User.Identity!.Name);

            return View();
        }
    }
}