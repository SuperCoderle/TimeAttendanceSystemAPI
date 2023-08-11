using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public EmployeesController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Employees
        [Roles("Administrator", "Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(Guid id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // GET: api/Employees/Authenticated
        [HttpGet("Authenticated")]
        public async Task<ActionResult<Employee>> GetEmployeeByAuth()
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }

            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized();
            }

            var user = await _context.TbUsers.FindAsync(Guid.Parse(User.FindFirstValue("id")!));
            if (user == null)
            {
                return BadRequest("User does not exists");
            }

            var employee = await _context.Employees.FindAsync(user.EmployeeID);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(Guid id, Employee employee)
        {
            if (id != employee.EmployeeID)
            {
                return BadRequest();
            }

            employee.LastUpdatedAt = DateTime.UtcNow;
            employee.LastUpdatedBy = User.FindFirstValue("fullname");

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'TimeAttendanceSystemContext.Employees'  is null.");
            }

            try
            {
                await CreateOrUpdate(employee);
            }
            catch (DbUpdateException)
            {
                if (EmployeeExists(employee.EmployeeID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEmployee", new { id = employee.EmployeeID }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            if (_context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            await DeleteUser(id);
            await DeletePayroll(id);
            await DeleteReport(id);
            await DeleteSchedule(id);

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task CreateOrUpdate(Employee employee)
        {
            try
            {
                employee.CreatedBy = User.FindFirstValue("fullname");

                if (_context.TbUsers.Any(x => x.EmployeeID == employee.EmployeeID))
                    _context.Employees.Update(employee);
                else
                    _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool EmployeeExists(Guid id)
        {
            return (_context.Employees?.Any(e => e.EmployeeID == id)).GetValueOrDefault();
        }

        private async Task DeleteUser(Guid empID)
        {
            if (_context.TbUsers == null)
            {
                return;
            }

            try
            {
                var user = await _context.TbUsers.Where(u => u.EmployeeID == empID).FirstOrDefaultAsync();
                if (user != null)
                {
                    await CancelRelationship(user);
                    _context.TbUsers.Remove(user);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DeletePayroll(Guid empID)
        {
            if (_context.Payrolls == null)
            {
                return;
            }

            try
            {
                var payroll = await _context.Payrolls.Where(x => x.EmployeeID == empID).FirstOrDefaultAsync();
                if (payroll != null)
                {
                    _context.Payrolls.Remove(payroll);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DeleteReport(Guid empID)
        {
            if (_context.Reports == null)
                return;
            try
            {
                var reports = await _context.Reports.Where(x => x.EmployeeID == empID).ToListAsync();
                if (reports.Any())
                {
                    foreach (var report in reports)
                    {
                        _context.Reports.Remove(report);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task DeleteSchedule(Guid empID)
        {
            if (_context.Reports == null)
                return;
            try
            {
                var schedules = await _context.Schedules.Where(x => x.EmployeeID == empID).ToListAsync();
                if (schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        _context.Schedules.Remove(schedule);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CancelRelationship(TbUser user)
        {
            try
            {
                var userRoles = await _context.UserRoles.Where(x => x.UserID == user.UserID).ToListAsync();
                if (userRoles.Any())
                {
                    foreach (var role in userRoles)
                    {
                        _context.UserRoles.Remove(role);
                    }
                }

                var refreshTokens = await _context.RefreshTokens.Where(x => x.UserID == user.UserID).ToListAsync();
                if (refreshTokens.Any())
                {
                    foreach (var refreshToken in refreshTokens)
                    {
                        _context.RefreshTokens.Remove(refreshToken);
                    }
                }

                var passwordChangeds = await _context.PasswordChangeds.Where(x => x.UserID == user.UserID).ToListAsync();
                if (passwordChangeds.Any())
                {
                    foreach (var passwordChanged in passwordChangeds)
                    {
                        _context.PasswordChangeds.Remove(passwordChanged);
                    }
                }


                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
