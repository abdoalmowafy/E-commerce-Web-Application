using Egost.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Egost.Data;
using Microsoft.EntityFrameworkCore;

namespace Egost.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class ManageController(EgostContext db) : Controller
    {
        private readonly EgostContext _db = db;
        private IEnumerable<string> GetCategoriesNames() => _db.Categories.Select(c => c.Name);




        // GET
        public IActionResult NewProduct()
        {
            ViewBag.Categories = GetCategoriesNames();
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewProduct(Product obj)
        {
            if (ModelState.IsValid)
            {
                // Add Product To Database before uploading media To Determine its Id
                _db.Products.Add(obj);
                _db.SaveChanges();

                // Create the directory to save Media Files
                string directoryPath = Path.Combine("wwwroot/ProductMedia/", obj.Id.ToString());
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

            ViewBag.Categories = GetCategoriesNames();
            return View(obj);
        }




        // GET
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
            string path = Path.Combine(@"wwwroot\ProductMedia\", OldProduct.Id.ToString());
            string[] fileNames = Directory.GetFiles(path);
            List<string> fileNamesOnly = fileNames.Select(filePath => Path.GetFileName(filePath)).ToList();
            
            ViewBag.FileNames = fileNamesOnly;
            ViewBag.Categories = GetCategoriesNames();

            return View(OldProduct);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product obj)
        {
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
        public IActionResult DeleteProduct(int Id, bool DeleteMedia)
        {
            var product = _db.Products.Find(Id);
            
            //Check If Product Exists
            if (product == null)
            {
                TempData["fail"] = "Product not found!";
                return RedirectToAction("Home", "Store");
            }

            product.DeletedDateTime = DateTime.Now;
            _db.Products.Update(product);
            _db.SaveChanges();

            // Delete All Product Media
            if (DeleteMedia)
            {
                string path = Path.Combine(@"wwwroot\ProductMedia\", Id.ToString());
                Directory.Delete(path, true);
            }

            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Home", "Store");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecoverProduct(int Id)
        {
            var product = _db.Products.Find(Id);

            //Check If Product Exists
            if (product == null)
            {
                TempData["fail"] = "Product not found!";
                return RedirectToAction("Home", "Store");
            }

            // Mark product as deleted
            product.DeletedDateTime = null;
            _db.Products.Update(product);
            _db.SaveChanges();

            // Delete All Product Media
            string path = Path.Combine(@"wwwroot\ProductMedia\", Id.ToString());
            Directory.Delete(path, true);

            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Home", "Store");
        }

        public IActionResult IndexUndeliveredOrders()
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name);
            IEnumerable<Order> orders = _db.Orders.Include(o => o.User);
            if (!User.IsInRole("Transporter"))
                orders = orders.Where(u => u.Transporter == user);
            
            return View(orders);
        }

        public IActionResult ViewOrder(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            Order order = _db.Orders
                .Include(o => o.Transporter)
                .Include(o => o.OrderProducts)
                .FirstOrDefault(o => o.Id == id)!;
            if (!User.IsInRole("Transporter") || order.Transporter == user)
            {
                IEnumerable<OrderProduct> orderProducts = order.OrderProducts;
                return View(orderProducts);
            }
            else
            {
                return RedirectToAction("IndexUndeliveredOrders");
            }
        }


        

        public IActionResult DeliveredOrder(int? id)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = _db.Orders.Include(o => o.Transporter).FirstOrDefault(o => o.Id == id);
            if (user == order.Transporter)
            {
                order.DeliveryDateTime = DateTime.Now;
                _db.Orders.Update(order);
                _db.SaveChanges();
                TempData["success"] = "Delivered Successfully!";
            }
            else
            {
                TempData["info"] = "Invalid Transporter!";
            }
            return RedirectToAction("IndexOrders");
        }
        public IActionResult ChartView(int? ProductId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            IEnumerable<OrderProduct> orderProducts = _db.OrderProducts
                .Include(op => op.Product)
                .Where(op => op.Product.Id == ProductId);

            IEnumerable<ReturnProduct> returnProducts = _db.ReturnProducts
                .Include(rp => rp.OrderProduct)
                    .ThenInclude(op => op.Product)
                .Where(rp => rp.OrderProduct.Product.Id == ProductId);

            return View((orderProducts, returnProducts));
        }

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
    }
}