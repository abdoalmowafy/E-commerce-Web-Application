using Azure.Core;
using Azure;
using Egost.Data;
using Egost.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Egost.Controllers
{
    [Authorize]
    public class OrderController(EgostContext db, IConfiguration configuration, HttpClient httpClient) : Controller
    {
        private readonly EgostContext _db = db;
        private readonly HttpClient _httpClient = httpClient;
        private readonly string ApiKey = configuration.GetSection("Paymob")["ApiKey"]!;
        private readonly int IntegrationId = int.Parse(configuration.GetSection("Paymob")["IntegrationId"]!);
        private readonly int IframeId = int.Parse(configuration.GetSection("Paymob")["Iframe1Id"]!);


        public IActionResult Index(string time)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            IEnumerable<Order> Orders = _db.Orders.Where(x => x.User == user);

            if (time == "Day") 
            {
                Orders = Orders.Where(u => (DateTime.Now - u.CreatedDateTime).Days < 1);
            }
            else if (time == "Week") 
            {
                Orders = Orders.Where(u => (DateTime.Now - u.CreatedDateTime).Days < 7);
            }
            else if (time == "Month") 
            { 
                Orders = Orders.Where(u => (DateTime.Now - u.CreatedDateTime).Days < 30);
            }
            else if (time == "Year") 
            { 
                Orders = Orders.Where(u => (DateTime.Now - u.CreatedDateTime).Days < 365);
            }
            return View(Orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(string PaymentMethod, bool DeliveryNeeded, string? identifier) 
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var cart = user.Cart;
            var promo = cart.PromoCode;              
            
            ulong totalCentsNoPromo = 0;
            foreach (var cartProduct in cart.CartProducts)
            {
                var SKU = cartProduct.Product.SKU;
                if (SKU < 1)
                {
                    return RedirectToAction("Index", "Cart");
                }
                else if (SKU < cartProduct.Quantity)
                {
                    return RedirectToAction("Index", "Cart");
                }
                totalCentsNoPromo += Convert.ToUInt64(cartProduct.Product.PriceCents * (1 - cartProduct.Product.SalePercent /100 ) * cartProduct.Quantity);
            }

            // Creating Order
            var orderProducts = new List<OrderProduct>(cart.CartProducts.Count);
            foreach (var cartProduct in cart.CartProducts)
            {
                var orderProduct = new OrderProduct {
                    Product = cartProduct.Product,
                    ProductPriceCents = cartProduct.Product.PriceCents,
                    SalePercent = cartProduct.Product.SalePercent,
                    Quantity = cartProduct.Quantity,
                };
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
                TotalCents = promo == null ? totalCentsNoPromo : Convert.ToUInt64(totalCentsNoPromo * (1 - promo.Percent / 100)),
                DeliveryNeeded = DeliveryNeeded,
                OrderProducts = orderProducts
            };
            _db.Orders.Add(order);
            cart.CartProducts = [];
            cart.PromoCode = null;
            _db.Carts.Update(cart);


            // Processing Order
            bool successful = true;
            if (PaymentMethod == "COD")
            {
                order.TotalCents += 1000;
            }
            else if (PaymentMethod == "CreditCard") 
            {
                string token = await PaymentApiFlow(order);
                CardPayment(token);
                var response = await _httpClient.GetAsync(BaseUrl);
            }
            else if (PaymentMethod == "MobileWallet")
            {
                string token = await PaymentApiFlow(order);
                await MobileWallet(token, identifier);
                var response = await _httpClient.GetAsync(BaseUrl);
            }


            if (successful)
            {
                // Successful Order: Adding Order To Db
                _db.SaveChanges();
                TempData["success"] = "Order submitted successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["fail"] = "Order wasn't successful, please try again later!";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? orderId)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = _db.Orders.Find(orderId);

            if (order == null || order.User != user)
            {
                return RedirectToAction("Index");
            }

            order.DeletedDateTime = DateTime.Now;
            _db.Orders.Update(order);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }



        // The base URL of Paymob API
        private const string BaseUrl = "https://accept.paymob.com/api";
        [HttpPost]
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
            // Check if the response is successfull
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
