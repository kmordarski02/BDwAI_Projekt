using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wypozyczalnia.Data;
using Wypozyczalnia.Models;

namespace Wypozyczalnia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EquipmentApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EquipmentItem>>> GetEquipmentItems()
        {
            return await _context.EquipmentItems.Include(e => e.Category).ToListAsync();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EquipmentItem>> GetEquipmentItem(int id)
        {
            var item = await _context.EquipmentItems.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EquipmentItem>> PostEquipmentItem(EquipmentItem equipmentItem)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.EquipmentItems.Add(equipmentItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEquipmentItem), new { id = equipmentItem.Id }, equipmentItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutEquipmentItem(int id, EquipmentItem equipmentItem)
        {
            if (id != equipmentItem.Id) return BadRequest();
            _context.Entry(equipmentItem).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.EquipmentItems.AnyAsync(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEquipmentItem(int id)
        {
            var item = await _context.EquipmentItems.FindAsync(id);
            if (item == null) return NotFound();
            _context.EquipmentItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
