using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryManagement.Models;
using Newtonsoft.Json;

namespace BakeryManagement.Controllers
{
    public class OrdersController : Controller
    {
        private readonly BakeryManagementContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersController(BakeryManagementContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Orders
        public async Task<IActionResult> Index(int? customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: ConfirmAddress
        [HttpGet]
        public async Task<IActionResult> ConfirmAddress()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Index", "Login", new { area = "CustomerUser" });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            return View(customer);
        }

        // POST: PlaceOrderWithAddress
        [HttpPost]
        public async Task<IActionResult> PlaceOrderWithAddress(string newAddress, string paymentMethod, string paymentMomo, string deliveryDate)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để đặt hàng.";
                return RedirectToAction("Index", "LoginC");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer != null && !string.IsNullOrEmpty(newAddress))
            {
                customer.Address = newAddress;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
            }

            var cartItems = await _context.Carts
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Cake)
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng của bạn hiện tại không có sản phẩm nào.";
                return RedirectToAction("Index", "Cart");
            }

            decimal totalAmount = cartItems.Sum(c => c.Quantity * c.Cake.Price);
            DateTime parsedDeliveryDate = DateTime.ParseExact(deliveryDate, "dd/MM/yyyy", null);

            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Đang xử lý",
                DeliveryDate = parsedDeliveryDate
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    CakeId = item.CakeId,
                    Quantity = item.Quantity,
                    Price = item.Cake.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            await _context.SaveChangesAsync();

            if (paymentMomo == "Chuyển khoản")
            {
                var paymentUrl = await CreateMoMoPayment(order.OrderId, totalAmount);
                if (!string.IsNullOrEmpty(paymentUrl))
                {
                    HttpContext.Session.SetInt32("PendingOrderId", order.OrderId);
                    return Redirect(paymentUrl); // Chuyển hướng đến trang MoMo để nhập thông tin thẻ
                }
                else
                {
                    TempData["Error"] = "Không thể tạo yêu cầu thanh toán MoMo.";
                    return Redirect("https://localhost:7050/Orders/ConfirmAddress");
                }
            }
            else
            {
                order.Status = "Tiền mặt";
                _context.Orders.Update(order);
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đơn hàng đã được đặt thành công.";
                return RedirectToAction("Index", new { customerId = customerId.Value });
            }
        }

        // Hàm tạo yêu cầu thanh toán MoMo (payWithATM)
        private async Task<string> CreateMoMoPayment(int orderId, decimal amount)
        {
            string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            string partnerCode = "MOMO_ATM_DEV";
            string accessKey = "w9gEg8bjA2AM2Cvr";
            string secretKey = "mD9QAVi4cm9N844jh5Y2tqjWaaJoGVFM";
            string orderInfo = $"Thanh toán đơn hàng #{orderId}";
            string redirectUrl = "https://localhost:7050/Orders/MoMoCallback";
            string ipnUrl = "https://localhost:7050/Orders/MoMoCallback";
            string requestId = Guid.NewGuid().ToString();
            string orderIdStr = $"MM{orderId}";
            string amountStr = ((int)amount).ToString();

            var rawData = $"accessKey={accessKey}&amount={amountStr}&extraData=&ipnUrl={ipnUrl}&orderId={orderIdStr}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType=payWithATM";
            var signature = HmacSha256(secretKey, rawData);

            var requestData = new
            {
                partnerCode,
                partnerName = "BakeryManagement",
                storeId = "BakeryStore",
                requestId,
                amount = amountStr,
                orderId = orderIdStr,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang = "vi",
                requestType = "payWithATM", // Thanh toán qua thẻ ATM
                autoCapture = true,
                extraData = "",
                signature
            };

            using var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                return responseData.payUrl;
            }

            return null;
        }

        // Callback từ MoMo
        [HttpGet]
        public async Task<IActionResult> MoMoCallback(string orderId, string resultCode, string message)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => "MM" + o.OrderId == orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return Redirect("https://localhost:7050/Orders/ConfirmAddress");
            }

            if (resultCode == "0")
            {
                order.Status = "Chuyển khoản";
                _context.Orders.Update(order);

                var cartItems = await _context.Carts
                    .Where(c => c.CustomerId == order.CustomerId)
                    .ToListAsync();
                _context.Carts.RemoveRange(cartItems);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Thanh toán MoMo thành công. Đơn hàng đã được đặt.";
                return Redirect("https://localhost:7050/Orders");
            }
            else
            {
                TempData["Error"] = $"Thanh toán MoMo thất bại: {message}";
                return Redirect("https://localhost:7050/Orders/ConfirmAddress");
            }
        }

        // Hàm tạo chữ ký HMAC SHA256
        private string HmacSha256(string key, string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}