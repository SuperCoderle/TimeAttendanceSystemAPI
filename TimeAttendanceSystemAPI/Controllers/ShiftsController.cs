using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeAttendanceSystemAPI.Models;
using TimeAttendanceSystemAPI.Services;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public ShiftsController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Shifts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShifts()
        {
            if (_context.Shifts == null)
            {
                return NotFound();
            }
            return await _context.Shifts.ToListAsync();
        }

        // GET: api/Shifts/5
        [Roles("Administrator", "Manager")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Shift>> GetShift(int id)
        {
            if (_context.Shifts == null)
            {
                return NotFound();
            }
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
            {
                return NotFound();
            }

            return shift;
        }

        // PUT: api/Shifts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShift(int id, Shift shift)
        {
            if (id != shift.ShiftID)
            {
                return BadRequest();
            }

            _context.Entry(shift).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Shifts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPost]
        public async Task<ActionResult<Shift>> PostShift(Shift shift)
        {
            if (_context.Shifts == null)
            {
                return Problem("Entity set 'TimeAttendanceSystemContext.Shifts'  is null.");
            }

            new CheckIdentService(_context).CheckIdentShift();

            _context.Shifts.Add(shift);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ShiftExists(shift.ShiftID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetShift", new { id = shift.ShiftID }, shift);
        }

        // DELETE: api/Shifts/5
        [Roles("Administrator", "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            if (_context.Shifts == null)
            {
                return NotFound();
            }

            await CancelRelationship(id);
            
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
            {
                return NotFound();
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return (_context.Shifts?.Any(s => s.ShiftID == id)).GetValueOrDefault();
        }

        private async Task CancelRelationship(int shiftID)
        {
            try
            {
                var schedules = await _context.Schedules.Where(x => x.ShiftID == shiftID).ToListAsync();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        schedule.ShiftID = null;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
