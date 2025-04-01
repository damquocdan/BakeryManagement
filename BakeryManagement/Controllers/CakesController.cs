using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BakeryManagement.Models;

namespace BakeryManagement.Controllers
{
    public class CakesController : Controller
    {
        private readonly BakeryManagementContext _context;

        public CakesController(BakeryManagementContext context)
        {
            _context = context;
        }

        // GET: Cakes
        public async Task<IActionResult> Index(string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;
            var cakes = from c in _context.Cakes select c;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                cakes = cakes.Where(c => c.Name.Contains(searchTerm) || c.Description.Contains(searchTerm));
            }

            return View(await cakes.ToListAsync());
        }

        // GET: Cakes/Details/5
        public IActionResult Details(int id)
        {
            var cake = _context.Cakes.FirstOrDefault(c => c.CakeId == id);
            if (cake == null)
            {
                return NotFound();
            }
            return PartialView("_CakeDetails", cake);
        }

        // GET: Cakes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cakes/Create (Thêm vào giỏ hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CakeId,Name,Description,Price,Category,Image,Stock")] Cake cake)
        {
            if (ModelState.IsValid)
            {
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                if (customerId == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để thêm bánh vào giỏ hàng.";
                    return RedirectToAction("Index", "LoginC");
                }

                // Xử lý hình ảnh từ dữ liệu Base64
                if (!string.IsNullOrEmpty(cake.Image) && cake.Image.Contains("data:image"))
                {
                    try
                    {
                        string fileName = $"{Guid.NewGuid()}.png";
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cakes", fileName);

                        // Tạo thư mục nếu chưa tồn tại
                        string directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Chuyển Base64 thành mảng byte và lưu file
                        byte[] imageBytes = Convert.FromBase64String(cake.Image.Split(',')[1]);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                        // Cập nhật đường dẫn ảnh vào thuộc tính Image
                        cake.Image = "/images/cakes/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("Image", $"Lỗi khi lưu hình ảnh: {ex.Message}");
                        return View(cake);
                    }
                }
                else
                {
                    // Nếu không có hình ảnh được cung cấp, gán một ảnh mặc định (tùy chọn)
                    cake.Image = "/images/cakes/default.png";
                }

                // Thêm bánh vào cơ sở dữ liệu
                _context.Add(cake);
                await _context.SaveChangesAsync();

                // Thêm vào giỏ hàng
                var cart = new Cart
                {
                    CustomerId = customerId.Value,
                    CakeId = cake.CakeId,
                    Quantity = 1
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Bánh đã được thêm vào giỏ hàng!";
                return RedirectToAction("Index", "Carts");
            }

            return View(cake);
        }

        private bool CakeExists(int id)
        {
            return _context.Cakes.Any(e => e.CakeId == id);
        }
    }
}