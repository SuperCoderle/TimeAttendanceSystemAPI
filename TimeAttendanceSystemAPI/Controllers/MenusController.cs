using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly TimeAttendanceSystemContext _context;

        public MenusController(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            if (_context.Menus == null)
            {
                return NotFound();
            }

            return await _context.Menus.ToListAsync();
        }

        // GET: api/Menus
        [HttpGet("Authenticate")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenusAuth()
        {
            if (_context.Menus == null)
            {
                return NotFound();
            }

            var role = User.IsInRole("Administrator") ? "Administrator" : User.IsInRole("Manager") ? "Manager" : "Employee";


            return await (from m in _context.Menus
                          join rm in _context.RoleMenus on m.MenuID equals rm.MenuID
                          join r in _context.Roles on rm.RoleID equals r.RoleID
                          where r.Name == role && m.IsActive
                          select m).ToListAsync();
        }

        // GET: api/Menus/Role/1
        [HttpGet("Role/{RoleID}")]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenusByRole(int RoleID)
        {
            if (_context.Menus == null)
            {
                return NotFound();
            }
            return await (from m in _context.Menus
                          join rm in _context.RoleMenus on m.MenuID equals rm.MenuID
                          where rm.RoleID == RoleID
                          select m).ToListAsync();
        }

        // GET: api/Menus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenu(int id)
        {
          if (_context.Menus == null)
          {
              return NotFound();
          }
            var menu = await _context.Menus.FindAsync(id);

            if (menu == null)
            {
                return NotFound();
            }

            return menu;
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenu(int id, Menu menu)
        {
            if (id != menu.MenuID)
            {
                return BadRequest();
            }

            _context.Entry(menu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
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

        // PUT: api/Menus/5?active=true
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}/Active")]
        public async Task<IActionResult> PutMenu(int id, string state, bool value)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            switch(state)
            {
                case "active":
                    menu.IsActive = value; 
                    break;
                case "submenu":
                    menu.IsSubmenu = value;
                    break;
            }

            _context.Entry(menu).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
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

        // POST: api/Menus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult<Menu>> PostMenu(Menu menu)
        {
          if (_context.Menus == null)
          {
              return Problem("Entity set 'TimeAttendanceSystemContext.Menus'  is null.");
          }
            int maxColumn = _context.Menus.OrderByDescending(x => x.MenuID).FirstOrDefault() != null ? _context.Menus.OrderByDescending(x => x.MenuID).FirstOrDefault()!.MenuID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Menu, RESEED, {maxColumn})");

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMenu", new { id = menu.MenuID }, menu);
        }

        // DELETE: api/Menus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            if (_context.Menus == null)
            {
                return NotFound();
            }

            await CancelRelationship(id);

            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MenuExists(int id)
        {
            return (_context.Menus?.Any(e => e.MenuID == id)).GetValueOrDefault();
        }

        private async Task CancelRelationship(int menuID)
        {
            try
            {
                var mrs = await _context.RoleMenus.Where(x => x.MenuID ==  menuID).ToListAsync();
                if(mrs.Any())
                {
                    foreach (var mr in mrs)
                    {
                        _context.RoleMenus.Remove(mr);
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
