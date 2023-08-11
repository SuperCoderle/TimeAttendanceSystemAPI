using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimeAttendanceSystemAPI.Models;
using TimeAttendanceSystemAPI.Services;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public ReportsController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Reports
        [Roles("Administrator", "Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports(int month)
        {
            await PostReport(month);

            if (_context.Reports == null)
            {
              return NotFound();
            }
            return await _context.Reports.Where(x => x.MonthlyReport == month).ToListAsync();
        }

        // GET: api/Reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
          if (_context.Reports == null)
          {
              return NotFound();
          }
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        // PUT: api/Reports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, Report report)
        {
            if (id != report.ReportID)
            {
                return BadRequest();
            }

            report.LastUpdatedAt = DateTime.UtcNow;
            report.LastUpdatedBy = User.FindFirstValue("fullname");
            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        private async Task PostReport(int month)
        {
            try
            {
                new CheckIdentService(_context).CheckIdentReport();

                foreach (var emp in _context.Employees)
                {
                    var schedules = await _context.Schedules.Where(x => x.EmployeeID == emp.EmployeeID && x.WorkDate.Month == month).ToListAsync();
                    

                    if (await _context.Reports.AnyAsync(x => x.EmployeeID == emp.EmployeeID && x.MonthlyReport == month))
                    {
                        var report = _context.Reports.Where(x => x.EmployeeID == emp.EmployeeID).FirstOrDefault();
                        
                        if (report != null)
                        {
                            report.GrossPay = Calculate(schedules, emp);
                            report.LastUpdatedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        Report newReport = new Report
                        {
                            ReportID = 0,
                            Title = $"Lương tháng {month}",
                            Description = null,
                            EmployeeID = emp.EmployeeID,
                            GrossPay = Calculate(schedules, emp),
                            MonthlyReport = month,
                            PaidStatus = "Chưa thanh toán",
                            CreatedAt = DateTime.Now,
                            LastUpdatedAt = null,
                            LastUpdatedBy = null,
                        };

                        _context.Reports.Add(newReport);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // DELETE: api/Reports/5
        [Roles("Administrator", "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            if (_context.Reports == null)
            {
                return NotFound();
            }
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportExists(int id)
        {
            return (_context.Reports?.Any(e => e.ReportID == id)).GetValueOrDefault();
        }

        private decimal Calculate(List<Schedule> schedules, Employee employee)
        {
            decimal total = 0;
            try
            {
                foreach (var sche in schedules)
                {
                    total += sche.TotalWorkHours;
                }
                decimal basicSalary = _context.Payrolls.Where(x => x.EmployeeID == employee.EmployeeID).FirstOrDefault()!.BasicSalary;

                total = total * basicSalary;
            }
            catch (Exception)
            {
                throw;
            }

            return total;
        }
    }
}
