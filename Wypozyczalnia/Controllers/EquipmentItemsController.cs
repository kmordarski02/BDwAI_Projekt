using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Wypozyczalnia.Data;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Controllers
{
    public class EquipmentItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipmentItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to populate ViewData used by views
        private void PopulateViewData()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
        }

        // GET: EquipmentItems
        public async Task<IActionResult> Index(string? season, string? category)
        {
            PopulateViewData();
            
            var items = _context.EquipmentItems.Include(e => e.Category).AsQueryable();

            if (!string.IsNullOrEmpty(season))
            {
                items = items.Where(i => i.Season == season);
                ViewData["CurrentSeason"] = season;
            }

            if (!string.IsNullOrEmpty(category))
            {
                items = items.Where(i => i.Category.Name == category);
            }

            return View(await items.ToListAsync());
        }

        // GET: EquipmentItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            PopulateViewData();
            var item = await _context.EquipmentItems.Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // GET: EquipmentItems/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateViewData();
            return View();
        }

        // POST: EquipmentItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Season,Quantity,CategoryId,TargetGender,Size,PricePerHour")] EquipmentItem equipmentItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(equipmentItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateViewData();
            return View(equipmentItem);
        }

        // GET: EquipmentItems/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.EquipmentItems.FindAsync(id);
            if (item == null) return NotFound();
            PopulateViewData();
            return View(item);
        }

        // POST: EquipmentItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Season,Quantity,CategoryId,TargetGender,Size,PricePerHour")] EquipmentItem equipmentItem)
        {
            if (id != equipmentItem.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(equipmentItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EquipmentItems.Any(e => e.Id == equipmentItem.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateViewData();
            return View(equipmentItem);
        }

        // GET: EquipmentItems/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            PopulateViewData();
            var item = await _context.EquipmentItems.Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: EquipmentItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.EquipmentItems.FindAsync(id);
            if (item != null)
            {
                _context.EquipmentItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
