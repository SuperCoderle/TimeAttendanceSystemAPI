using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly TimeAttendanceSystemContext context;

        public LoginController(IConfiguration config, TimeAttendanceSystemContext context)
        {
            this.config = config;
            this.context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login([FromQuery] UserLogin user)
        {
            try
            {
                var token = GenegrateToken(user);
                if (token == null)
                {
                    return Unauthorized("Your email or password is not correct!");
                }
                return Ok(new JwtToken { token = token });
            }
            catch (Exception)
            {
                throw;
            }
        }


        private string GenegrateToken(UserLogin user)
        {
            var users = (from u in context.TbUsers
                        join ur in context.UserRoles on u.UserID equals ur.UserID
                        join r in context.Roles on ur.RoleID equals r.RoleID
                        where user.Email == u.Email
                        select new { u.UserID, u.Fullname, r.Name, u.Password }).ToList();

            if(users.Count() == 0)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(user.Password, users.FirstOrDefault().Password))
            {
                return null;
            }

            UpdateLogin(users.FirstOrDefault().UserID);

            var userExist = users.FirstOrDefault();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>()
            {
                new Claim("fullname", userExist.Fullname),
                new Claim("id", userExist.UserID.ToString())
            };

            foreach(var u in users)
            {
                claims.Add(new Claim("roles", u.Name));
            }

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMonths(2),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private void UpdateLogin(Guid id)
        {
            var user = context.TbUsers.Find(id);
            user.LastLoggedIn = DateTime.Now;

            context.Entry(user).Property(x => x.LastLoggedIn).IsModified = true;
            context.SaveChanges();
        }
    }
}
