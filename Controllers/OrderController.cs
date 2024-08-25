using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Egost.Controllers
{
    public class OrderController(EgostContext db, IConfiguration configuration, HttpClient httpClient) : Controller
    {
        private readonly EgostContext _db = db;
        private readonly HttpClient _httpClient = httpClient;
        private readonly string ApiKey = configuration.GetSection("Paymob")["ApiKey"]!;
        private readonly int IntegrationId = int.Parse(configuration.GetSection("Paymob")["IntegrationId"]!);
        private readonly int IframeId = int.Parse(configuration.GetSection("Paymob")["Iframe1Id"]!);

        private static readonly string[] availablePaymentMethods = ["CreditCard", "MobileWallet", "COD"];

        [Authorize]
        [Route("Order")]
        public IActionResult Index()
        {
            var user = _db.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                .Include(u => u.Orders)
                    .ThenInclude(o => o.PromoCode)
                .FirstOrDefault(u => u.UserName == User.Identity!.Name);

            IEnumerable<Order> Orders = user.Orders
                .Where(o => o.Processed).Reverse();

            return View(Orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> NewOrder(string PaymentMethod, bool DeliveryNeeded, string? identifier, int ShippingAddressId = 1)
        {
            var user = _db.Users
                .Include(u => u.Orders)
                .Include(u => u.Cart)
                    .ThenInclude(c => c.CartProducts)
                        .ThenInclude(cp => cp.Product)
                .Include(u => u.Cart)
                    .ThenInclude(c => c.PromoCode)
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.UserName == User.Identity!.Name)!;

            if(user.Orders.Any(o => !o.DeliveryDateTime.HasValue))
            {
                TempData["fail"] = "Can't have two orders at one time!\nContact us!";
                return RedirectToAction("Index", "Contact");
            }

            var cart = user.Cart;
            var cartProducts = cart.CartProducts;
            var promo = cart.PromoCode;
            Address? address = _db.Addresses.Find(ShippingAddressId);

            if (cartProducts.IsNullOrEmpty() || // Possible cartProducts errors!
                cartProducts.Any(cp => cp.Quantity < 1 || cp.Product.SKU < cp.Quantity || cp.Product.DeletedDateTime.HasValue) ||
                (promo != null && (promo.DeletedDateTime.HasValue || !promo.Active)) || // Possible promocode errors!
                address == null || (!address.StoreAddress && !user.Addresses.Contains(address)) || // Possible input errors!
                !availablePaymentMethods.Contains(PaymentMethod))
            {
                TempData["fail"] = "Something went wrong!";
                return RedirectToAction("Index", "Cart");
            }

            bool NeedProcessing = true;
            ulong Fee = 0;
            if (PaymentMethod == "COD")
            {
                Fee += 1000;
                NeedProcessing = false;
            }

            
            // Creating Order
            ulong totalCentsNoPromo = 0;
            var orderProducts = new List<OrderProduct>(cartProducts.Count);
            foreach (var cartProduct in cartProducts)
            {
                var orderProduct = new OrderProduct {
                    Product = cartProduct.Product,
                    ProductPriceCents = cartProduct.Product.PriceCents,
                    SalePercent = cartProduct.Product.SalePercent,
                    Quantity = cartProduct.Quantity,
                    Warranty = cartProduct.Product.Warranty,
                };
                totalCentsNoPromo += cartProduct.Product.PriceCents * cartProduct.Quantity * Convert.ToByte(100 - cartProduct.Product.SalePercent) / 100;
                _db.OrderProducts.Add(orderProduct);
                orderProducts.Add(orderProduct);
                cartProduct.Product.SKU -= cartProduct.Quantity;
                _db.Products.Update(cartProduct.Product);
            }

            Order order = new()
            {
                User = user,
                PromoCode = promo,
                PaymentMethod = PaymentMethod,
                Processed = !NeedProcessing,
                TotalCents = Fee + (promo == null ? totalCentsNoPromo : totalCentsNoPromo * Convert.ToByte(100 - promo.Percent) / 100),
                DeliveryNeeded = DeliveryNeeded,
                OrderProducts = orderProducts,
                Address = address
            };

            // Processing Order
            if (PaymentMethod == "COD")
            {
                _db.Orders.Add(order);
                _db.SaveChanges();
                TempData["success"] = "Order was processed successfully!";
                return RedirectToAction("Index");
            }
            else if (PaymentMethod == "CreditCard") 
            {
                string token = await PaymentApiFlow(order);
                return CardPayment(token);
            }
            else
            {
                string token = await PaymentApiFlow(order);
                return await MobileWallet(token, identifier!);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void PaymobResponse(string response)
        {
            var responseObject = JsonSerializer.Deserialize<dynamic>(response);
            int? OrderId = int.Parse(responseObject.GetSection["order"]);
            var order = _db.Orders
                .Include(o => o.User)
                    .ThenInclude(u => u.Cart)
                        .ThenInclude(c => c.CartProducts)
                .Include(o => o.OrderProducts)
                    .ThenInclude(cp => cp.Product)
                .First(o => o.PaymobOrderId == OrderId);
            var cart = order.User.Cart;

            if (responseObject.GetSection["success"])
            {
                // Flag order as processed
                order.Processed = true;
                _db.Orders.Update(order);

                // Empty Cart
                _db.CartProducts.RemoveRange(cart.CartProducts);
                cart.CartProducts = [];
                cart.PromoCode = null;
                _db.Carts.Update(cart);
            }
            else
            {
                // Delete order
                foreach (OrderProduct orderProduct in order.OrderProducts)
                {
                    orderProduct.Product.SKU += orderProduct.Quantity;
                    _db.Products.Update(orderProduct.Product);
                    _db.OrderProducts.Remove(orderProduct);
                }
                _db.Orders.Remove(order);
            }
            _db.SaveChanges();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Delete(int? OrderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity!.Name);
            var order = _db.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)  
                .FirstOrDefault(o => o.Id == OrderId);

            if (order == null || order.User != user)
            {
                return RedirectToAction("Index");
            }

            // Delete order
            foreach (OrderProduct orderProduct in order.OrderProducts)
            {
                orderProduct.Product.SKU += orderProduct.Quantity;
                _db.Products.Update(orderProduct.Product);
            }
            order.DeletedDateTime = DateTime.Now;
            _db.Orders.Update(order);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult NewReturnProductOrder(int? OrderId, int? OrderProductId, string ReturnReason, uint Quantity = 1)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = _db.Orders.Include(o => o.User).Include(o => o.OrderProducts).FirstOrDefault(o => o.Id == OrderId);
            var orderProduct = order.OrderProducts.FirstOrDefault(op => op.Id == OrderProductId);

            if (order == null || order.User != user || orderProduct == null || orderProduct.Quantity < Quantity 
                || order.CreatedDateTime + orderProduct.Warranty < DateTime.Now || ReturnReason.IsNullOrEmpty())
            {
                return Redirect("/");
            }

            _db.ReturnProductOrders.Add(new()
            {
                Order = order,
                OrderProduct = orderProduct,
                Quantity = Quantity,
                ReturnReason = ReturnReason,
            });
            orderProduct.PartiallyOrFullyReturnedDateTime = DateTime.Now;
            _db.OrderProducts.Update(orderProduct);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // The base URL of Paymob API
        private const string BaseUrl = "https://accept.paymob.com/api";
        private async Task<string> PaymentApiFlow(Order order)
        {
            // Step One: Authentication Request
            string auth_token;
            var request1 = new
            {
                api_key = ApiKey
            };
            var requestJson = JsonSerializer.Serialize(request1);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/auth/tokens", requestContent);
            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var responseJson = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON string to a dynamic object
                var responseObject = JsonSerializer.Deserialize<dynamic>(responseJson);

                // Return the token property of the response object as a string
                auth_token = responseObject.GetProperty("token").GetString();
            }
            else
            {
                // Throw an exception if the response is not successful
                throw new Exception($"Authentication Request failed at step one: {response.StatusCode}");
            }


            // Step Two: Order Registration API
            int id;
            var request2 = new
            {
                auth_token,
                delivery_needed = order.DeliveryNeeded.ToString(),
                amount_cents = order.TotalCents.ToString(),
                currency = order.Currency,
                //items = _db.OrderItems.Where(x => x.OrderId == order.Id),
                items = Array.Empty<string>(),
            };
            requestJson = JsonSerializer.Serialize(request2);
            requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            response = await _httpClient.PostAsync($"{BaseUrl}/ecommerce/orders", requestContent);
            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var responseJson = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON string to a dynamic object
                var responseObject = JsonSerializer.Deserialize<dynamic>(responseJson);

                // Return the token property of the response object as a string
                id = responseObject.GetProperty("id").GetInt32();
            }
            else
            {
                // Throw an exception if the response is not successful
                throw new Exception($"Authentication Request failed at step two: {response.StatusCode}");
            }



            // Step Three: Payment Key Request
            var request3 = new
            {
                auth_token,
                amount_cents = order.TotalCents.ToString(),
                expiration = 3600,
                order_id = id.ToString(),
                billing_data = new
                {
                    apartment = "803",
                    email = "claudette09@exa.com",
                    floor = "42",
                    first_name = "Clifford",
                    street = "Ethan Land",
                    building = "8028",
                    phone_number = "+86(8)9135210487",
                    shipping_method = "PKG",
                    postal_code = "01898",
                    city = "Jaskolskiburgh",
                    country = "CR",
                    last_name = "Nicolas",
                    state = "Utah"
                },
                currency = order.Currency,
                integration_id = IntegrationId
            };
            requestJson = JsonSerializer.Serialize(request3);
            requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            response = await _httpClient.PostAsync($"{BaseUrl}/acceptance/payment_keys", requestContent);
            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Submit order to db
                order.PaymobOrderId = id;
                _db.Orders.Add(order);
                _db.SaveChanges();


                // Read the response content as a JSON string
                var responseJson = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON string to a dynamic object
                var responseObject = JsonSerializer.Deserialize<dynamic>(responseJson);

                // Return the token property of the response object as a string
                return responseObject.GetProperty("token").GetString();
            }
            else
            {
                // Throw an exception if the response is not successful
                throw new Exception($"Authentication Request failed at step three: {response.StatusCode}");
            }
        }
        private RedirectResult CardPayment(string token)
        {
            string IframeURL = $"https://accept.paymobsolutions.com/api/acceptance/iframes/{IframeId}?payment_token={token}";
            return Redirect(IframeURL);
        }
        private async Task<IActionResult> MobileWallet(string token, string identifier)
        {
            string RedirectionURL;
            var request = new
            {
                source = new
                {
                    identifier,
                    subtype = "WALLET"
                },
                payment_token = token
            };
            var requestJson = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{BaseUrl}/acceptance/payments/pay", requestContent);
            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a JSON string
                var responseJson = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON string to a dynamic object
                var responseObject = JsonSerializer.Deserialize<dynamic>(responseJson);

                // Return the token property of the response object as a string
                RedirectionURL = responseObject.GetProperty("redirection_url").GetString();
            }
            else
            {
                // Throw an exception if the response is not successful
                throw new Exception($"Authentication Request failed: {response.StatusCode}");
            }
            return Redirect(RedirectionURL);
        }
    }
}
