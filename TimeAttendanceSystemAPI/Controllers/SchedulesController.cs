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
    public class SchedulesController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public SchedulesController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/Schedules
        [Roles("Administrator", "Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
          if (_context.Schedules == null)
          {
              return NotFound();
          }
            return await _context.Schedules.OrderByDescending(x => !x.IsSubmit).OrderByDescending(x => x.WorkDate).ToListAsync();
        }

        // GET: api/Schedules/5
        [HttpGet("Employee")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedulesByEmp()
        {
            if (_context.Schedules == null)
            {
                return NotFound();
            }

            Guid? userID = Guid.Parse(User.FindFirstValue("id")!);
            if (userID == null)
            {
                return Unauthorized();
            } 

            try
            {
                var user = await _context.TbUsers.FindAsync(userID);
                if (user == null)
                {
                    return Unauthorized(userID);
                }

                return await _context.Schedules.Where(x => x.EmployeeID == user.EmployeeID).OrderByDescending(x => !x.IsSubmit).OrderByDescending(x => x.WorkDate).ToListAsync();
            }
            catch (Exception)

            {
                throw;
            }


        }

        // GET: api/Schedules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(Guid id)
        {
            if (_context.Schedules == null)
            {
                return NotFound();
            }
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            return schedule;
        }

        // GET: api/Schedules/5
        [HttpGet("GetToday")]
        public async Task<ActionResult<Schedule>> GetScheduleForEmp(Guid employeeID)
        {
            if (_context.Schedules == null)
            {
                return NotFound();
            }

            Guid? userID = Guid.Parse(User.FindFirstValue("id")!);
            if(userID == null)
            {
                return Unauthorized();
            }
            var user = await _context.TbUsers.FindAsync(userID);
            if (user == null)
            {
                return Unauthorized(userID);
            }

            var schedule = await _context.Schedules.FirstOrDefaultAsync(x => x.IsSubmit == true && x.EmployeeID == user.EmployeeID && x.WorkDate == DateTime.Today && x.IsSubmit);

            if (schedule != null)
                await CheckOnTime(schedule);

            return schedule == null ? NoContent() : schedule;
        }

        // PUT: api/Schedules/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSchedule(Guid id, string action, Schedule schedule)
        {
            if (id != schedule.ScheduleID)
            {
                return BadRequest();
            }

            switch(action)
            {
                case "Update":
                    schedule.ApprovedAt = DateTime.Today;
                    schedule.ApprovedBy = User.FindFirstValue("fullname");
                    _context.Entry(schedule).State = EntityState.Modified;
                    break;
                case "Check":
                    Check(schedule);
                    break;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
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

        private void Check(Schedule sche)
        {
            var shift = _context.Shifts.Find(sche.ShiftID);
            if (shift == null)
                return;
            if (TimeSpan.Compare(new TimeSpan(0, 0, 0), sche.TimeIn) == 0 && TimeSpan.Compare(new TimeSpan(0, 0, 0), sche.TimeOut) == 0)
            {
                if (TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks - shift.StartTime.Ticks).TotalMinutes > 10)
                    sche.ViolationID = _context.Violations.FirstOrDefault(x => x.ViolationError == "Đi trễ")?.ViolationID;
                sche.TimeIn = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            else if(TimeSpan.Compare(new TimeSpan(0, 0, 0), sche.TimeIn) != 0)
            {
                sche.TimeOut = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                sche.TotalWorkHours = (decimal)TimeSpan.FromTicks(sche.TimeOut.Ticks - sche.TimeIn.Ticks).TotalHours;
                if(sche.TotalWorkHours <= 0)
                    sche.ViolationID = _context.Violations.FirstOrDefault(x => x.ViolationError == "Không chấm công ra")?.ViolationID;
            }
            sche.IsOpen = false;
            _context.Schedules.Entry(sche).State = EntityState.Modified;
        }

        // POST: api/Schedules
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Schedule>> PostSchedule(Schedule schedule)
        {
            if (_context.Schedules == null)
            {
              return Problem("Entity set 'TimeAttendanceSystemContext.Schedules' is null.");
            }
            schedule.CreatedBy = User.FindFirstValue("fullname");
            _context.Schedules.Add(schedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ScheduleExists(schedule.ScheduleID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchedule", new { id = schedule.ScheduleID }, schedule);
        }

        [HttpPost("Employee")]
        public async Task<ActionResult<Schedule>> PostScheduleFromEmployee(Schedule schedule)
        {
            if (_context.Schedules == null)
            {
                return Problem("Entity set 'TimeAttendanceSystemContext.Schedules' is null.");
            }

            var user = await _context.TbUsers.FindAsync(Guid.Parse(User.FindFirstValue("id")!));
            if (user == null)
                return NotFound();

            schedule.EmployeeID = (Guid)user.EmployeeID!;
            schedule.CreatedBy = User.FindFirstValue("fullname");
            _context.Schedules.Add(schedule);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ScheduleExists(schedule.ScheduleID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSchedule", new { id = schedule.ScheduleID }, schedule);
        }

        // DELETE: api/Schedules/5
        [Roles("Administrator", "Manager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            if (_context.Schedules == null)
            {
                return NotFound();
            }
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task CheckOnTime(Schedule schedule)
        {
            try
            {
                var shift = await _context.Shifts.FindAsync(schedule.ShiftID);
                if (shift == null)
                    return;

                if(TimeSpan.Compare(schedule.TimeIn, new TimeSpan(0,0,0)) == 0)
                {
                    if(0 <= (int)TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks - shift.StartTime.Ticks).TotalMinutes && (int)TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks - shift.StartTime.Ticks).TotalMinutes <= 30)
                    {
                        schedule.IsOpen = true;
                        _context.Schedules.Update(schedule);
                    }

                }
                else if (TimeSpan.Compare(schedule.TimeOut, new TimeSpan(0, 0, 0)) == 0)
                {
                    if (0 <= (int)TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks - shift.EndTime.Ticks).TotalMinutes && (int)TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks - shift.EndTime.Ticks).TotalMinutes <= 30)
                    {
                        schedule.IsOpen = true;
                        _context.Schedules.Update(schedule);
                    }
                }
                else
                {
                    schedule.IsOpen = false;
                    _context.Schedules.Update(schedule);
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

        private bool ScheduleExists(Guid id)
        {
            return (_context.Schedules?.Any(e => e.ScheduleID == id)).GetValueOrDefault();
        }
    }
}
