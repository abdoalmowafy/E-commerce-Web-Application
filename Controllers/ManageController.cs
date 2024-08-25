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


        [Authorize(Roles = "Admin,Moderator,Transporter")]
        public IActionResult IndexAllOrders(bool undeliveredOnly = false)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name);
            IEnumerable<Order> orders = _db.Orders.Include(o => o.Transporter);
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

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ChartView(int? ProductId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            IEnumerable<OrderProduct> orderProducts = _db.OrderProducts
                .Include(op => op.Product)
                .Where(op => op.Product.Id == ProductId);

            IEnumerable<ReturnProductOrder> returnProductOrders = _db.ReturnProductOrders
                .Include(rpo => rpo.OrderProduct)
                    .ThenInclude(op => op.Product)
                .Where(rpo => rpo.OrderProduct.Product.Id == ProductId);

            return View((orderProducts, returnProductOrders));
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