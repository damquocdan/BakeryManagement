using BakeryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Areas.AdminManagement.Controllers {

    public class DashboardController : BaseController
    {
        private readonly BakeryManagementContext _context;

        public DashboardController(BakeryManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Truyền dữ liệu vào ViewData hoặc ViewBag
            ViewData["TotalOrders"] = await _context.Orders.CountAsync();
            ViewData["TotalCustomers"] = await _context.Customers.CountAsync();
            ViewData["TotalEmployees"] = await _context.Employees.CountAsync();
            ViewData["TotalProducts"] = await _context.Cakes.CountAsync();

            ViewData["LatestOrders"] = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }

}
