using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly TimeAttendanceSystemContext context;
        private readonly TokenValidationParameters refreshTokenValidationParams;

        public LoginController(IConfiguration config, TimeAttendanceSystemContext context, TokenValidationParameters refreshTokenValidationParams)
        {
            this.config = config;
            this.context = context;
            this.refreshTokenValidationParams = refreshTokenValidationParams;
        }

        [HttpPost]
        public async Task<ActionResult> Login([FromQuery] UserLogin user)
        {
            try
            {
                var userExists = await context.TbUsers.FirstOrDefaultAsync(x => x.Email == user.Email);
                if (userExists == null)
                    return Unauthorized("Sai tên đăng nhập.");

                if (!BCrypt.Net.BCrypt.Verify(user.Password, userExists.Password))
                    return Unauthorized("Sai mật khẩu.");


                var userToken = GenegrateToken(userExists);

                if (userToken == null)
                {
                    return Unauthorized("Your email or password is not correct!");
                }

                if (userToken.refreshToken != null)
                {
                    await context.RefreshTokens.AddAsync(userToken.refreshToken);
                    await context.SaveChangesAsync();
                }

                return Ok(new JwtToken
                {
                    Token = userToken.token,
                    RefreshToken = userToken.refreshToken?.Token,
                    isSuccess = true
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(TokenRequest tokenRequest)
        {
            if(ModelState.IsValid)
            {
                var res = await VerifyToken(tokenRequest);

                if(res == null) 
                {
                    return BadRequest("Invalid token");
                }

                return Ok(res);
            }

            return BadRequest("Invalid payload");
        }

        [HttpPut("RevokeToken/{refreshToken}")]
        public async Task<IActionResult> RevokeToken(string refreshToken)
        {
            if (ModelState.IsValid)
            {
                var rt = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
                if (rt == null)
                    return NotFound();
                rt.IsRevoked = true;
            }
            try
            {
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private UserToken GenegrateToken(TbUser user)
        {
            var userRoles = (from ur in context.UserRoles
                         join r in context.Roles on ur.RoleID equals r.RoleID
                         where user.UserID == ur.UserID
                         select r.Name).ToList();

            UpdateLogin(user.UserID);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>()
            {
                new Claim("fullname", user.Fullname),
                new Claim("id", user.UserID.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach(var r in userRoles)
            {
                claims.Add(new Claim("roles", r));
            }

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
                );

            var refreshToken = new RefreshToken()
            {
                JwtID = token.Id,
                IsUsed = false,
                UserID = user.UserID,
                DateAdded = DateTime.UtcNow,
                DateExpiry = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = RandomString(25) + Guid.NewGuid()
            };



            return new UserToken { 
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshToken
            };
        }

        private async Task<JwtToken?> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {



                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, refreshTokenValidationParams, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken) 
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase);

                    if(result == false)
                    {
                        return null;
                    }
                }

                var utcExpireDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value);

                var expDate = UnixTimeStampToDateTime(utcExpireDate);

                if(expDate > DateTime.UtcNow)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                var storedRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if(storedRefreshToken == null)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                if(DateTime.UtcNow > storedRefreshToken.DateExpiry)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                if(storedRefreshToken.IsUsed)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                if(storedRefreshToken.IsRevoked)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;

                if(storedRefreshToken.JwtID != jti)
                {
                    return new JwtToken()
                    {
                        isSuccess = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                context.RefreshTokens.Update(storedRefreshToken);
                await context.SaveChangesAsync();

                var user = await context.TbUsers.FindAsync(storedRefreshToken.UserID);
                if (user == null)
                    return null;
                var userToken = GenegrateToken(user);
                return new JwtToken()
                {
                    Token = userToken.token,
                    RefreshToken = userToken.refreshToken?.Token,
                    isSuccess = true
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void UpdateLogin(Guid id)
        {
            var user = context.TbUsers.Find(id);
            if (user == null)
            {
                return;
            }

            user.LastLoggedIn = DateTime.Now;

            context.Entry(user).Property(x => x.LastLoggedIn).IsModified = true;
            context.SaveChanges();
        }

        private static string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}
