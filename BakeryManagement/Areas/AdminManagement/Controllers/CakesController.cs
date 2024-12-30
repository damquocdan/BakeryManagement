using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BakeryManagement.Models;

namespace BakeryManagement.Areas.AdminManagement.Controllers
{
    [Area("AdminManagement")]
    public class CakesController : Controller
    {
        private readonly BakeryManagementContext _context;

        public CakesController(BakeryManagementContext context)
        {
            _context = context;
        }

        // GET: AdminManagement/Cakes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cakes.ToListAsync());
        }

        // GET: AdminManagement/Cakes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cake = await _context.Cakes
                .FirstOrDefaultAsync(m => m.CakeId == id);
            if (cake == null)
            {
                return NotFound();
            }

            return View(cake);
        }

        // GET: AdminManagement/Cakes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminManagement/Cakes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CakeId,Name,Description,Price,Category,Image,Stock")] Cake cake)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Any() && files[0].Length > 0)
                {
                    var file = files[0];
                    var fileName = file.FileName;
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\cakes", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                        cake.Image = "/images/cakes/" + fileName;
                    }
                }
                _context.Add(cake);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cake);
        }

        // GET: AdminManagement/Cakes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cake = await _context.Cakes.FindAsync(id);
            if (cake == null)
            {
                return NotFound();
            }
            return View(cake);
        }

        // POST: AdminManagement/Cakes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CakeId,Name,Description,Price,Category,Image,Stock")] Cake cake)
        {
            if (id != cake.CakeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var files = HttpContext.Request.Form.Files;
                    if (files.Any() && files[0].Length > 0)
                    {
                        var file = files[0];
                        var fileName = file.FileName;
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\cakes", fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                            cake.Image = "/images/cakes/" + fileName;
                        }
                    }
                    _context.Update(cake);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CakeExists(cake.CakeId))
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
            return View(cake);
        }

        // GET: AdminManagement/Cakes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cake = await _context.Cakes
                .FirstOrDefaultAsync(m => m.CakeId == id);
            if (cake == null)
            {
                return NotFound();
            }

            return View(cake);
        }

        // POST: AdminManagement/Cakes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cake = await _context.Cakes.FindAsync(id);
            if (cake != null)
            {
                _context.Cakes.Remove(cake);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CakeExists(int id)
        {
            return _context.Cakes.Any(e => e.CakeId == id);
        }
    }
}
