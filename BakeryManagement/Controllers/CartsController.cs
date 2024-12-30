using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BakeryManagement.Models;

namespace BakeryManagement.Controllers
{
    public class CartsController : Controller
    {
        private readonly BakeryManagementContext _context;

        public CartsController(BakeryManagementContext context)
        {
            _context = context;
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            var bakeryManagementContext = _context.Carts.Include(c => c.Cake).Include(c => c.Customer).Where(c=>c.CustomerId == customerId);
            return View(await bakeryManagementContext.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            // Get CustomerId from session
            var customerId = HttpContext.Session.GetInt32("CustomerId");

            // Check if CustomerId exists in session
            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để sử dụng chức năng này.";
                return RedirectToAction("Index", "LoginC");
            }

            // Check if the cake already exists in the cart
            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CakeId == id && c.CustomerId == customerId);

            if (existingCartItem != null)
            {
                // If the cake is already in the cart, increase the quantity by 1
                existingCartItem.Quantity += 1;

                // Update the cart in the database
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                // If the cake is not in the cart, create a new cart item
                var cart = new Cart
                {
                    CustomerId = customerId.Value, // CustomerId from session
                    CakeId = id, // CakeId to properly reference the cake
                    Quantity = 1 // Default quantity is 1
                };

                // Add the cake to the cart
                await _context.Carts.AddAsync(cart);
            }
            // Save changes to the database
            await _context.SaveChangesAsync();

            // Success message
            TempData["Success"] = "Bánh đã được thêm vào giỏ hàng!";

            // Redirect to the cart page
            return RedirectToAction("Index", "Home");
        }



        // GET: Carts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Cake)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["CakeId"] = new SelectList(_context.Cakes, "CakeId", "CakeId");
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartId,CustomerId,CakeId,Quantity,Price,AddedAt")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CakeId"] = new SelectList(_context.Cakes, "CakeId", "CakeId", cart.CakeId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", cart.CustomerId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["CakeId"] = new SelectList(_context.Cakes, "CakeId", "CakeId", cart.CakeId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", cart.CustomerId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,CustomerId,CakeId,Quantity,Price,AddedAt")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CakeId"] = new SelectList(_context.Cakes, "CakeId", "CakeId", cart.CakeId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", cart.CustomerId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Cake)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> updatedQuantities)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để cập nhật giỏ hàng.";
                return RedirectToAction("Index", "Login", new { area = "CustomerUser" });
            }

            // Iterate through each item in the dictionary of updated quantities
            foreach (var item in updatedQuantities)
            {
                // Find the corresponding cart item for the customer
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.CartId == item.Key && c.CustomerId == customerId);

                if (cartItem != null)
                {
                    // Update the quantity of the cart item
                    cartItem.Quantity = item.Value;
                    _context.Carts.Update(cartItem);
                }
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Provide feedback to the user
            TempData["Success"] = "Giỏ hàng đã được cập nhật.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");

            if (customerId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xóa bánh khỏi giỏ hàng.";
                return RedirectToAction("Index", "Login", new { area = "CustomerUser" });
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CartId == id && c.CustomerId == customerId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bánh đã được xóa khỏi giỏ hàng.";
            }

            return RedirectToAction("Index");
        }

    }
}
