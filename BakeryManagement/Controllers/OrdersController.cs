using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryManagement.Models;

namespace BakeryManagement.Controllers
{
    public class OrdersController : Controller
    {
        private readonly BakeryManagementContext _context;

        public OrdersController(BakeryManagementContext context)
        {
            _context = context;
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

        // Action to confirm or update address before placing an order
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

            return View(customer); // Pass customer model to the view
        }

        // Action to handle placing the order after confirming the address
        [HttpPost]
        public async Task<IActionResult> PlaceOrderWithAddress(string newAddress, string paymentMethod,DateTime? deliveryDate)
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
                // Update customer's address if a new one is provided
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

            // Calculate the total amount
            decimal totalAmount = cartItems.Sum(c => c.Quantity * c.Cake.Price);

            // Create the order
            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = paymentMethod == "Chuyển khoản" ? "Chuyển khoản" : "Tiền mặt", // Set status based on payment method
                DeliveryDate = deliveryDate ?? DateTime.Now.AddDays(1)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order details for each item in the cart
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

            // Save the order details to the database
            await _context.SaveChangesAsync();

            // Optionally: Clear the customer's cart after placing the order
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đơn hàng đã được đặt thành công.";
            return RedirectToAction("Index", "Orders", new { CustomerId = customerId });
        }


        // POST: Update Cart
        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> updatedQuantities)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để cập nhật giỏ hàng.";
                return RedirectToAction("Index", "LoginC");
            }

            foreach (var item in updatedQuantities)
            {
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CartId == item.Key && c.CustomerId == customerId);

                if (cartItem != null)
                {
                    cartItem.Quantity = item.Value;
                    _context.Carts.Update(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Giỏ hàng đã được cập nhật.";
            return RedirectToAction("Index", "Cart");
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
