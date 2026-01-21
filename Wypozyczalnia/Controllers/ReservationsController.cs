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
        // POST: Reservations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentItemId,From,To,IsStudent,StudentEmail")] Reservation reservation)
        {
            // 1. Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            reservation.ApplicationUserId = user.Id;

            // Remove errors for properties we set manually or ignore
            ModelState.Remove(nameof(Reservation.ApplicationUserId));
            ModelState.Remove(nameof(Reservation.TotalPrice));
            ModelState.Remove(nameof(Reservation.EquipmentItem));
            ModelState.Remove(nameof(Reservation.ApplicationUser));

            // 2. Validate Dates
            if (reservation.From >= reservation.To)
            {
                ModelState.AddModelError("To", "Data zwrotu musi być późniejsza niż data wypożyczenia.");
            }

            // 2. Validate Student Checkbox and Email
            if (reservation.IsStudent)
            {
                if (string.IsNullOrWhiteSpace(reservation.StudentEmail))
                {
                    ModelState.AddModelError("StudentEmail", "Mail studencki jest wymagany gdy zaznaczono opcję studenta.");
                }
                else
                {
                    var emailParts = reservation.StudentEmail.Split('@');
                    if (emailParts.Length < 2 || !emailParts[1].Contains("student", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("StudentEmail", "Domena maila musi zawierać słowo 'student'.");
                    }
                }
            }

            // 3. Get Equipment and Validate Existence
            var equipment = await _context.EquipmentItems.FindAsync(reservation.EquipmentItemId);
            if (equipment == null)
            {
                ModelState.AddModelError("EquipmentItemId", "Wybrany sprzęt nie istnieje.");
            }

            if (ModelState.IsValid && equipment != null)
            {
                // 4. Check Availability (Overlapping)
                var overlappingReservationsCount = await _context.Reservations
                    .Where(r => r.EquipmentItemId == reservation.EquipmentItemId &&
                                r.From < reservation.To && reservation.From < r.To)
                    .CountAsync();

                if (overlappingReservationsCount >= equipment.Quantity)
                {
                    ModelState.AddModelError(string.Empty, "W podanym terminie wybrany sprzęt nie jest dostępny, ponieważ wszystkie sztuki zostały już wypożyczone.");
                }
                else
                {
                    // 5. Calculate Total Price
                    var totalHours = (decimal)(reservation.To - reservation.From).TotalHours;
                    var basePrice = totalHours * equipment.PricePerHour;
                    
                    if (reservation.IsStudent)
                    {
                        reservation.TotalPrice = basePrice * 0.8m; // 20% discount
                    }
                    else
                    {
                        reservation.TotalPrice = basePrice;
                    }

                    // 6. Save
                    _context.Add(reservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got here, something failed
            ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
            return View(reservation);
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
                ModelState.AddModelError(string.Empty, "Data i godzina zakończenia musi być późniejsza niż data rozpoczęcia.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // Student Validation for Edit
            if (reservation.IsStudent)
            {
                if (string.IsNullOrWhiteSpace(reservation.StudentEmail))
                {
                    ModelState.AddModelError("StudentEmail", "Mail studencki jest wymagany gdy zaznaczono opcję studenta.");
                }
                else
                {
                    var emailParts = reservation.StudentEmail.Split('@');
                    if (emailParts.Length < 2 || !emailParts[1].Contains("student", StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("StudentEmail", "Domena maila musi zawierać słowo 'student'.");
                    }
                }
                if (ModelState.ErrorCount > 0)
                {
                    ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                    return View(reservation);
                }
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
                ModelState.AddModelError(string.Empty, "W podanym terminie wybrany sprzęt nie jest dostępny, ponieważ wszystkie sztuki zostały już wypożyczone.");
                ViewData["EquipmentItems"] = _context.EquipmentItems.ToList();
                return View(reservation);
            }

            // 5. Calculate Total Price (Recalculate on Edit)
            var totalHoursOnEdit = (decimal)(reservation.To - reservation.From).TotalHours;
            var basePriceOnEdit = totalHoursOnEdit * equipment.PricePerHour;
            if (reservation.IsStudent)
            {
                reservation.TotalPrice = basePriceOnEdit * 0.8m;
            }
            else
            {
                reservation.TotalPrice = basePriceOnEdit;
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
