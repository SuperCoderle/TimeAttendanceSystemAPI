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
    public class TbUsersController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public TbUsersController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        // GET: api/TbUsers
        [Authorize]
        [Roles("Administrator", "Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TbUser>>> GetTbUsers()
        {
          if (_context.TbUsers == null)
          {
              return NotFound();
          }
            return await _context.TbUsers.ToListAsync();
        }

        // GET: api/TbUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TbUser>> GetTbUser(Guid id)
        {
          if (_context.TbUsers == null)
          {
              return NotFound();
          }
            var tbUser = await _context.TbUsers.FindAsync(id);

            if (tbUser == null)
            {
                return NotFound();
            }

            return tbUser;
        }

        // GET: api/TbUsers/5
        [HttpGet("Authenticated")]
        public async Task<ActionResult<TbUser>> GetTbUserAuth()
        {
            if (_context.TbUsers == null)
            {
                return NotFound();
            }
            var tbUser = await _context.TbUsers.FindAsync(Guid.Parse(User.FindFirstValue("id")!));

            if (tbUser == null)
            {
                return NotFound();
            }

            return tbUser;
        }

        // PUT: api/TbUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTbUser(Guid id, TbUser tbUser)
        {
            if (id != tbUser.UserID)
            {
                return BadRequest();
            }

            _context.Entry(tbUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TbUserExists(id))
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

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(Password password)
        {
            Guid userID = Guid.Parse(User.FindFirstValue("id")!);

            var user = await _context.TbUsers.FindAsync(userID);

            if(user == null)
            {
                return BadRequest();
            }

            if(!BCrypt.Net.BCrypt.Verify(password.oldPassword, user.Password))
            {
                return Conflict("Sai mật khẩu");
            }

            int maxColumn = _context.PasswordChangeds.OrderByDescending(x => x.PasswordChangedID).FirstOrDefault() != null ? _context.PasswordChangeds.OrderByDescending(x => x.PasswordChangedID).FirstOrDefault().PasswordChangedID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (PasswordChanged, RESEED, {maxColumn})");

            var passwordChanged = new PasswordChanged() { UserID = userID, OldPassword = user.Password };
            _context.PasswordChangeds.Add(passwordChanged);

            user.Password = BCrypt.Net.BCrypt.HashPassword(password.newPassword);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/TbUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Roles("Administrator", "Manager")]
        [HttpPost]
        public async Task<ActionResult<TbUser>> Register(TbUser tbUser)
        {
            if (_context.TbUsers == null)
            {
              return Problem("Entity set 'TimeAttendanceSystemContext.TbUsers'  is null.");
            }

            else if(TbUserExists(tbUser.Email))
            {
                return Conflict("Email này đã được tạo. Vui lòng nhập một email khác!");
            }

            else if(tbUser.EmployeeID != null)
            {
                var newEmployee = new Employee
                {
                    EmployeeID = (Guid)tbUser.EmployeeID,
                    Fullname = "",
                    Birthday = DateTime.UtcNow,
                    Gender = "",
                    PhoneNumber = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "",
                    LastUpdatedAt = null,
                    LastUpdatedBy = null,
                };
                _context.Employees.Add(newEmployee);
            }

            tbUser.Password = BCrypt.Net.BCrypt.HashPassword(tbUser.Password);

            _context.TbUsers.Add(tbUser);

            if(tbUser.IsManager)
            {
                var roleID = (from r in _context.Roles where r.Name == "Manager" select r.RoleID).FirstOrDefault();
                _context.UserRoles.Add(new UserRole { UserID = tbUser.UserID, RoleID = roleID });
            }
            else
            {
                var roleID = (from r in _context.Roles where r.Name == "Employee" select r.RoleID).FirstOrDefault();
                _context.UserRoles.Add(new UserRole { UserID = tbUser.UserID, RoleID = roleID });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TbUserExists(tbUser.UserID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTbUser", new { id = tbUser.UserID }, tbUser);
        }

        // DELETE: api/TbUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTbUser(Guid id)
        {
            if (_context.TbUsers == null)
            {
                return NotFound();
            }

            await CancelRelationship(id);

            var tbUser = await _context.TbUsers.FindAsync(id);
            if (tbUser == null)
            {
                return NotFound();
            }

            _context.TbUsers.Remove(tbUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TbUserExists(Guid id)
        {
            return (_context.TbUsers?.Any(e => e.UserID == id)).GetValueOrDefault();
        }

        private bool TbUserExists(string email)
        {
            return (_context.TbUsers?.Any(x => x.Email == email)).GetValueOrDefault();
        }

        private async Task CancelRelationship(Guid userID)
        {
            try
            {
                var urs = await _context.UserRoles.Where(x => x.UserID == userID).ToListAsync();
                if (urs.Any())
                {
                    foreach (var ur in urs)
                    {
                        _context.UserRoles.Remove(ur);
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
