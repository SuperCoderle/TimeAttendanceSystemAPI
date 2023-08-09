using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Roles("Administrator", "Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ViolationsController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public ViolationsController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Violations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Violation>>> GetViolations()
        {
          if (_context.Violations == null)
          {
              return NotFound();
          }
            return await _context.Violations.ToListAsync();
        }

        // GET: api/Violations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Violation>> GetViolation(int id)
        {
          if (_context.Violations == null)
          {
              return NotFound();
          }
            var violation = await _context.Violations.FindAsync(id);

            if (violation == null)
            {
                return NotFound();
            }

            return violation;
        }

        // PUT: api/Violations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutViolation(int id, Violation violation)
        {
            if (id != violation.ViolationID)
            {
                return BadRequest();
            }

            _context.Entry(violation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ViolationExists(id))
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

        // POST: api/Violations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Violation>> PostViolation(Violation violation)
        {
            if (_context.Violations == null)
            {
              return Problem("Entity set 'TimeAttendanceSystemContext.Violations'  is null.");
            }

            int maxColumn = _context.Violations.OrderByDescending(x => x.ViolationID).FirstOrDefault() != null ? _context.Violations.OrderByDescending(x => x.ViolationID).FirstOrDefault().ViolationID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Violation, RESEED, {maxColumn})");

            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetViolation", new { id = violation.ViolationID }, violation);
        }

        // DELETE: api/Violations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            if (_context.Violations == null)
            {
                return NotFound();
            }

            await CancelRelationship(id);

            var violation = await _context.Violations.FindAsync(id);
            if (violation == null)
            {
                return NotFound();
            }

            _context.Violations.Remove(violation);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task CancelRelationship(int violationID)
        {
            try
            {
                var schedules = await _context.Schedules.Where(x => x.ViolationID == violationID).ToListAsync();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        schedule.ViolationID = null;
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

        private bool ViolationExists(int id)
        {
            return (_context.Violations?.Any(e => e.ViolationID == id)).GetValueOrDefault();
        }
    }
}
