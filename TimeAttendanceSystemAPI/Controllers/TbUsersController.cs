using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
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

        // POST: api/TbUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TbUser>> Register(TbUser tbUser)
        {
            if (_context.TbUsers == null)
            {
              return Problem("Entity set 'TimeAttendanceSystemContext.TbUsers'  is null.");
            }

            if(TbUserExists(tbUser.Email))
            {
                return Conflict("Email này đã được tạo. Vui lòng nhập một email khác!");
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
    }
}
