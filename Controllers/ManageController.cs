using Egost.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Egost.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Egost.Areas.Identity.Data;
using AspNetCoreGeneratedDocument;

namespace Egost.Controllers
{
    public class ManageController(EgostContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly EgostContext _db = db;
        private readonly UserManager<User> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        private IEnumerable<string> GetCategoriesNames() => _db.Categories.Select(c => c.Name);

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult StoreRadar(
        string? editorUsername,
        string? editedType,
        string? editedId,
        string? editedField,
        string? oldData,
        string? newData,
        DateTime? start,
        DateTime? end,
        int pageIndex = 1) {
            var edits = _db.EditHistories.Include(eh => eh.Editor).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(editorUsername)) edits = edits.Where(e => e.Editor.UserName.Contains(editorUsername));
            if (!string.IsNullOrWhiteSpace(editedType)) edits = edits.Where(e => e.EditedType.Contains(editedType));
            if (!string.IsNullOrWhiteSpace(editedId)) edits = edits.Where(e => e.EditedId.Contains(editedId));
            if (!string.IsNullOrWhiteSpace(editedField)) edits = edits.Where(e => e.EditedField.Contains(editedField));
            if (!string.IsNullOrWhiteSpace(oldData)) edits = edits.Where(e => e.OldData.Contains(oldData));
            if (!string.IsNullOrWhiteSpace(newData)) edits = edits.Where(e => e.NewData.Contains(newData));
            if (start != null) edits = edits.Where(e => e.EditDateTime > start);
            if (end != null) edits = edits.Where(e => e.EditDateTime < end);

            ViewData["editorUsername"] = editorUsername;
            ViewData["editedType"] = editedType;
            ViewData["editedId"] = editedId;
            ViewData["editedField"] = editedField;
            ViewData["oldData"] = oldData;
            ViewData["newData"] = newData;
            ViewData["start"] = start;
            ViewData["end"] = end;
            return View(edits.ToPaginatedList(pageIndex, 30));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult DeletesRadar(
        string? deleterUsername,
        string? deletedType,
        int? deletedId,
        DateTime? start,
        DateTime? end,
        int pageIndex = 1)
        {
            var deletes = _db.DeletesHistory.Include(eh => eh.Deleter).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(deleterUsername)) deletes = deletes.Where(d => d.Deleter.UserName.Contains(deleterUsername));
            if (!string.IsNullOrWhiteSpace(deletedType)) deletes = deletes.Where(d => d.DeletedType.Contains(deletedType));
            if (deletedId.HasValue) deletes = deletes.Where(d => d.DeletedId == deletedId.Value);
            if (start != null) deletes = deletes.Where(e => e.DeleteDateTime > start);
            if (end != null) deletes = deletes.Where(e => e.DeleteDateTime < end);

            ViewData["deleterUsername"] = deleterUsername;
            ViewData["deletedType"] = deletedType;
            ViewData["deletedId"] = deletedId;
            ViewData["start"] = start;
            ViewData["end"] = end;
            return View(deletes.ToPaginatedList(pageIndex, 30));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> IndexUsers(string? role, string targetUser = "", int pageIndex = 1)
        {
            var users = _db.Users
                .Include(u => u.Orders)
                .Include(u => u.ReturnProductOrders)
                .Where(u => u.NormalizedUserName.Contains(targetUser.ToUpper()));


            var userRoles = new List<(string, string, string, IList<string> roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                bool hasRole = string.IsNullOrWhiteSpace(role) || roles.Contains(role);
                
                if (hasRole) userRoles.Add(new(user.Id, user.Name, user.Email, roles));
            }

            ViewData["Roles"] = _roleManager.Roles;
            ViewData["role"] = role;
            ViewData["targetUser"] = targetUser;
            return View(userRoles.ToPaginatedList(pageIndex, 20));
        }



        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> GiveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest($"Role '{roleName}' does not exist.");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                TempData["success"] = $"'{roleName}' role added to user '{user.UserName}' successfully";
                return RedirectToAction(nameof(IndexUsers));
            }

            return BadRequest("Error adding role. " + string.Join(", ", result.Errors));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> RemoveRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID '{userId}' not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin")) return Forbid();

            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (result.Succeeded)
            {
                TempData["success"] = $"All roles removed from user '{user.UserName}' successfully";
                return RedirectToAction(nameof(IndexUsers));
            }

            return BadRequest("Error removing roles. " + string.Join(", ", result.Errors));
        }




        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult IndexCategories()
        {
            return View(_db.Categories.Include(c => c.Products));
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewCategories(string categoryName)
        {
            if(categoryName.IsNullOrEmpty() || GetCategoriesNames().Any(cn => cn.Contains(categoryName, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["fail"] = "Add unused category name!";
            }
            else
            {
                _db.Categories.Add(new Category { Name = categoryName });
                TempData["info"] = $"{categoryName} category added successfully!";
            }
            return RedirectToAction(nameof(IndexCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategories(int categoryId)
        {
            Category? category = _db.Categories.Include(c => c.Products).FirstOrDefault(c => c.Id == categoryId);
            if (category == null)
            {
                return NotFound();
            }

            if (category.Products.Count > 0)
            {
                TempData["fail"] = "Can't delete category having one or more products!";
            }
            else
            {
                _db.Categories.Remove(category);
                _db.DeletesHistory.Add(new() { 
                    Deleter = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name),
                    DeletedType = $"{nameof(Category)}: {category.Name}",
                    DeletedId = 0});
                _db.SaveChanges();
            }

            
            return RedirectToAction(nameof(IndexCategories));
        }



        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult IndexPromoCodes()
        {
            var usedInOrders = _db.Orders.Include(o => o.PromoCode).Where(o => o.PromoCode != null)
                .GroupBy(o => o.PromoCode.Id).ToDictionary(x => x.Key, y => y.Count());
            var usedInCarts = _db.Carts.Include(c => c.PromoCode).Where(o => o.PromoCode != null)
                .GroupBy(c => c.PromoCode.Id).ToDictionary(x => x.Key, y => y.Count());

            IEnumerable<(PromoCode, int, int)> promoCodes = _db.PromoCodes.ToList()
                .Select(promoCode => (
                promoCode,
                usedInOrders.TryGetValue(promoCode.Id, out var uio) ? uio : 0,
                usedInCarts.TryGetValue(promoCode.Id, out var uic) ? uic : 0
            ));
            return View(promoCodes);
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewPromoCode(string code, string description, int percent, long? maxSaleCents)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description) || percent < 1 || percent > 99 || _db.PromoCodes.Any(p => string.Equals(p.Code, code)))
            {
                TempData["fail"] = "Add unused PromoCode!";
            }
            else
            {
                _db.PromoCodes.Add(new PromoCode
                {
                    Code = code,
                    Description = description,
                    Percent = percent,
                    MaxSaleCents = maxSaleCents,
                    Active = true
                });

                _db.SaveChanges();
            }
            return RedirectToAction(nameof(IndexPromoCodes));
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeactivatePromoCode(int promoId)
        {
            PromoCode? promo = _db.PromoCodes.Find(promoId);
            if (promo == null)
            {
                return NotFound();
            }

            promo.Active = false;
            _db.PromoCodes.Update(promo);
            _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

            return RedirectToAction(nameof(IndexPromoCodes));
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActivatePromoCode(int promoId)
        {
            PromoCode? promo = _db.PromoCodes.Find(promoId);
            if (promo == null)
            {
                return NotFound();
            }

            promo.Active = true;
            _db.PromoCodes.Update(promo);
            _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

            return RedirectToAction(nameof(IndexPromoCodes));
        }

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
                _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

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

            // Mark product as deleted
            product.DeletedDateTime = DateTime.Now;
            _db.Products.Update(product);
            _db.DeletesHistory.Add(new() {
                Deleter = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name),
                DeletedType = nameof(Product),
                DeletedId = Id
            });
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
            _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));

            TempData["success"] = "Product recovered successfully!";
            return RedirectToAction("Home", "Store");
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ChartView()
        {
            Dictionary<string, int> searchedWords = _db.Searches.Where(s => !string.IsNullOrEmpty(s.KeyWord)).AsEnumerable()
                .SelectMany(s => s.KeyWord.Split(' ', StringSplitOptions.RemoveEmptyEntries)).GroupBy(w => w)
                .Select(g => (g.Key, g.Count())).OrderByDescending(g => g.Item2).ToDictionary();


            Dictionary<string, int> SearchedCategories = _db.Searches.Include(s => s.Category).Where(s => s.Category != null)
                .AsEnumerable().GroupBy(s => s.Category.Name).Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(g => g.Value).ToDictionary();

            IEnumerable<dynamic> productMetrics = _db.Products.Select(product => new
            {
                product.Id,
                product.Name,
                TotalSales = _db.OrderProducts.Where(op => op.Product.Id == product.Id).Sum(op => op.Quantity),
                Revenue = _db.OrderProducts.Where(op => op.Product.Id == product.Id).Sum(op => op.ProductPriceCents * (1 - op.SalePercent / 100.0) * op.Quantity),
                StockLevel = product.SKU,
                ProductViews = product.Views,
                CartCount = _db.CartProducts.Where(cp => cp.Product.Id == product.Id).Count(),
                WishlistCount = _db.Users.Where(u => u.WishList.Any(p => p.Id == product.Id)).Count(),
                ReturnRate = _db.ReturnProductOrders.Count(rpo => rpo.OrderProduct.Product.Id == product.Id) / (double)(_db.OrderProducts.Count(op => op.Product.Id == product.Id) + 1),
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
            });

            return View((productMetrics,searchedWords,SearchedCategories));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult ProductCharts(int id, DateTime starting)
        {
            var product = _db.Products
                .Include(p => p.Reviews)
                .Include(p => p.Category)
                .Include(p => p.EditsHistory)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            var cartCount = _db.CartProducts.Include(cp => cp.Product).Count(cp => cp.Product.Id == id);

            var wishlistCount = _db.Users.Include(u => u.WishList).Count(u => u.WishList.Any(p => p.Id == id));

            var Views = product.EditsHistory.Count(e => e.EditedField == nameof(product.Views) && e.EditDateTime > starting);
            
            var orderProducts = _db.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product)
                .Where(o => o.CreatedDateTime > starting).SelectMany(o => o.OrderProducts)
                .Where(op => op.Product.Id == id);

            var totalSales = orderProducts.Sum(op => op.Quantity);

            var revenue = orderProducts.Sum(op => op.ProductPriceCents * (1 - (op.SalePercent / 100.0)) * op.Quantity);

            var returnCount = _db.ReturnProductOrders.Include(rpo => rpo.OrderProduct).ThenInclude(op => op.Product)
                .Count(rpo => rpo.OrderProduct.Product.Id == id && rpo.CreatedDateTime > starting);

            var Ratings = new ulong[5];
            foreach (var review in product.Reviews) if(review.CreatedDateTime > starting) Ratings[review.Rating - 1]++;

            var productMetrics = new
            {
                product.Id,
                product.Name,
                cartCount,
                wishlistCount,
                Views,
                product.SKU,
                totalSales,
                revenue,
                returnCount,
                Ratings
            };

            ViewBag.starting = starting;
            return View(productMetrics);
        }



        // Orders Managment
        [Authorize(Roles = "Admin,Moderator,Transporter")]
        public async Task<IActionResult> OrdersDashboard(string? targetUser, int? status, int pageIndex = 1)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            var orders = _db.Orders
                .Include(o => o.Transporter)
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Where(o => o.Status != OrderStatus.Paying).AsEnumerable();

            var returnProductOrders = _db.ReturnProductOrders
                .Include(rpo => rpo.Address)
                .Include(rpo => rpo.Transporter)
                .Include(rpo => rpo.Order)
                    .ThenInclude(o => o.User)
                .Include(rpo => rpo.OrderProduct)
                    .ThenInclude(op => op.Product).AsEnumerable();

            if (!string.IsNullOrEmpty(targetUser))
            {
                orders = orders.Where(o => o.User.UserName == targetUser);
                returnProductOrders = returnProductOrders.Where(rpo => rpo.Order.User.UserName == targetUser);
            }

            if (User.IsInRole("Transporter"))
            {
                orders = orders.Where(u => u.Transporter == user);
                returnProductOrders = returnProductOrders.Where(u => u.Transporter == user);
            }

            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == (OrderStatus)status+1);
                returnProductOrders = returnProductOrders.Where(rpo => rpo.Status == (ReturnStatus)status);
            }

            ViewData["availableTransporters"] = (await _userManager.GetUsersInRoleAsync("Transporter")).Select(u => new { u.Id, u.Name, u.Email });
            ViewData["targetUser"] = targetUser;
            ViewData["status"] = status;
            return View((orders.ToPaginatedList(pageIndex, 20), returnProductOrders.ToPaginatedList(pageIndex, 20)));
        }


        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignTransporterToOrder(int orderId, string transporterId)
        {
            var order = _db.Orders.Include(o => o.User).Include(o => o.Transporter).FirstOrDefault(o => o.Id == orderId); 
            if (order == null) return NotFound();

            var transporter = _db.Users.FirstOrDefault(u => u.Id == transporterId);
            if (transporter == null) return NotFound();

            if (order.Status != OrderStatus.Processing) return BadRequest();

            order.Transporter = transporter;
            order.TransporterId = transporterId;
            order.Status = OrderStatus.OnTheWay;
            _db.Orders.Update(order);
            _db.SaveChanges();

            return RedirectToAction(nameof(OrdersDashboard), new { targetUser = order.User.Email });
        }



        [Authorize(Roles = "Admin,Moderator,Transporter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delivered(int? orderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = _db.Orders.Include(o => o.Transporter).FirstOrDefault(o => o.Id == orderId);
            if (order == null || order.Status != OrderStatus.OnTheWay
                || (User.IsInRole("Transporter") && order.Transporter != user))
            {
                TempData["info"] = "Something went wrong!";
            }
            else
            {
                order.DeliveryDateTime = DateTime.Now;
                order.Status = OrderStatus.Delivered;
                _db.Orders.Update(order);
                _db.SaveChanges();
                TempData["success"] = "Delivered Successfully!";
            }
            return RedirectToAction(nameof(OrdersDashboard));
        }


        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignTransporterToReturnProductOrder(int returnProductOrderId, string transporterId)
        {
            var returnProductOrder = _db.ReturnProductOrders.Include(o => o.Order).ThenInclude(o => o.User).Include(rpo => rpo.Transporter).FirstOrDefault(rpo => rpo.Id == returnProductOrderId);
            if (returnProductOrder == null) return NotFound();

            var transporter = _db.Users.FirstOrDefault(u => u.Id == transporterId);
            if (transporter == null) return NotFound();

            if (returnProductOrder.Status != ReturnStatus.Processing) return BadRequest();

            returnProductOrder.Transporter = transporter;
            returnProductOrder.Status = ReturnStatus.OnTheWay;
            _db.ReturnProductOrders.Update(returnProductOrder);

            return RedirectToAction(nameof(OrdersDashboard), new { targetUser = returnProductOrder.Order.User.Email });
        }


        [Authorize(Roles = "Admin,Moderator,Transporter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Returned(int? returnProductOrderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var returnProductOrder = _db.ReturnProductOrders
                .Include(rpo => rpo.Transporter)
                .Include(rpo => rpo.Order)
                .Include(rpo => rpo.OrderProduct)
                    .ThenInclude(op => op.Product)
                .FirstOrDefault(rpo => rpo.Id == returnProductOrderId);
            var order = returnProductOrder.Order;
            var orderProduct = returnProductOrder.OrderProduct;
            var product = returnProductOrder.OrderProduct.Product;

            if (returnProductOrder == null || returnProductOrder.ReturnedDateTime != null || returnProductOrder.DeletedDateTime != null
                || order == null || order.Status != OrderStatus.Delivered 
                || (User.IsInRole("Transporter") && returnProductOrder.Transporter != user))
            {
                TempData["info"] = "Something went wrong!";
            }
            else
            {
                product.SKU += returnProductOrder.Quantity;
                _db.Products.Update(product);

                orderProduct.PartiallyOrFullyReturnedDateTime = DateTime.Now;
                _db.OrderProducts.Update(orderProduct);

                returnProductOrder.ReturnedDateTime = DateTime.Now;
                returnProductOrder.Status = ReturnStatus.Returned;
                _db.ReturnProductOrders.Update(returnProductOrder);

                _db.SaveChanges();
                TempData["success"] = "Returned Successfully!";
            }
            return RedirectToAction(nameof(OrdersDashboard));
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
                _db.SaveChanges(_db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name));
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
            _db.DeletesHistory.Add(new()
            {
                Deleter = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name),
                DeletedType = nameof(Address),
                DeletedId = storeAddressId
            });
            _db.SaveChanges();
            return View(storeAddress);
        }
    }
}