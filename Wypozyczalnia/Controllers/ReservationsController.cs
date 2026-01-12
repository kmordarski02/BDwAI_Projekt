using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wypozyczalnia.Data;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var reservations = _context.Reservations.Include(r => r.EquipmentItem).Include(r => r.ApplicationUser);
            return View(await reservations.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations
                .Include(r => r.EquipmentItem)
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create(int? equipmentId)
        {
            ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
            ViewData["SelectedEquipmentId"] = equipmentId;
            return View();
        }

        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            // ModelState validation
            if (!ModelState.IsValid)
            {
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // Date validation
            if (reservation.From >= reservation.To)
            {
                ModelState.AddModelError(string.Empty, "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // Ensure equipment exists
            var equipment = await _context.EquipmentItems.FindAsync(reservation.EquipmentItemId);
            if (equipment == null)
            {
                ModelState.AddModelError(string.Empty, "Wybrany sprzęt nie istnieje.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // Check overlapping reservations
            var overlappingReservations = await _context.Reservations
                .Where(r => r.EquipmentItemId == reservation.EquipmentItemId &&
                            r.From < reservation.To && reservation.From < r.To)
                .ToListAsync();

            if (overlappingReservations.Count >= equipment.Quantity)
            {
                ModelState.AddModelError(string.Empty, "Brak dostępności sprzętu w wybranym terminie.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // Get current user safely
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Nie można pobrać informacji o użytkowniku. Zaloguj się ponownie.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            reservation.ApplicationUserId = user.Id;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Reservations/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return NotFound();
            ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            if (reservation.From >= reservation.To)
            {
                ModelState.AddModelError(string.Empty, "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            var equipment = await _context.EquipmentItems.FindAsync(reservation.EquipmentItemId);
            if (equipment == null)
            {
                ModelState.AddModelError(string.Empty, "Wybrany sprzęt nie istnieje.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            var overlappingReservations = await _context.Reservations
                .Where(r => r.EquipmentItemId == reservation.EquipmentItemId &&
                            r.Id != reservation.Id &&
                            r.From < reservation.To && reservation.From < r.To)
                .ToListAsync();

            if (overlappingReservations.Count >= equipment.Quantity)
            {
                ModelState.AddModelError(string.Empty, "Brak dostępności sprzętu w wybranym terminie.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            try
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Reservations.AnyAsync(e => e.Id == reservation.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Reservations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var reservation = await _context.Reservations.Include(r => r.EquipmentItem).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
