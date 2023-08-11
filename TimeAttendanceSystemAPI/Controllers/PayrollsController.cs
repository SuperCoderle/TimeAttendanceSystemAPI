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
    public class PayrollsController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public PayrollsController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Payrolls
        [Roles("Administrator", "Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payroll>>> GetPayrolls()
        {
            if (_context.Payrolls == null)
            {
                return NotFound();
            }
            return await _context.Payrolls.ToListAsync();
        }

        // GET: api/Payrolls/5
        [Roles("Administrator", "Manager")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Payroll>> GetPayroll(int id)
        {
            if (_context.Payrolls == null)
            {
                return NotFound();
            }
            var payroll = await _context.Payrolls.FindAsync(id);

            if (payroll == null)
            {
                return NotFound();
            }

            return payroll;
        }

        // GET: api/Payrolls/5
        [HttpGet("Employee")]
        public async Task<ActionResult<Payroll>> GetPayrollByEmployee(Guid id)
        {
            if (_context.Payrolls == null)
            {
                return NotFound();
            }
            var payroll = await _context.Payrolls.Where(x => x.EmployeeID == id).FirstOrDefaultAsync();

            if (payroll == null)
            {
                return NotFound();
            }

            return payroll;
        }

        // PUT: api/Payrolls/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPayroll(int id, Payroll payroll)
        {
            if (id != payroll.PayRollID)
            {
                return BadRequest();
            }

            _context.Entry(payroll).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PayrollExists(id))
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

        // POST: api/Payrolls
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPost]
        public async Task<ActionResult<Payroll>> PostPayroll(Payroll payroll)
        {
            if (_context.Payrolls == null)
            {
                return Problem("Entity set 'TimeAttendanceSystemContext.Payrolls'  is null.");
            }

            new CheckIdentService(_context).CheckIdentPayroll();

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPayroll", new { id = payroll.PayRollID }, payroll);
        }

        // DELETE: api/Payrolls/5
        [Roles("Administrator", "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayroll(int id)
        {
            if (_context.Payrolls == null)
            {
                return NotFound();
            }
            var payroll = await _context.Payrolls.FindAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }

            _context.Payrolls.Remove(payroll);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PayrollExists(int id)
        {
            return (_context.Payrolls?.Any(e => e.PayRollID == id)).GetValueOrDefault();
        }
    }
}
