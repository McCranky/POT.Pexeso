using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Data;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        PexesoDataContext _context;
        public AuthController(PexesoDataContext dataContext)
        {
            _context = dataContext;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDisplayInfo>> LoginUser([FromBody] string nickname)
        {
            var usr = await _context.Users.FirstOrDefaultAsync(usr => usr.Nickname == nickname);

            if (usr != null && usr.IsOnline) {
                return BadRequest(null);
            }

            if (usr == null) {
                usr = new Data.Models.User { IsOnline = true, Loses = 0, Wins = 0, Nickname = nickname };
                await _context.Users.AddAsync(usr);
                await _context.SaveChangesAsync();
            } else {
                usr.IsOnline = true;
                _context.Update(usr);
                await _context.SaveChangesAsync();
            }

            // vytvoriť claim
            var claims = new Claim(ClaimTypes.Name, nickname);
            // vytvorit claimIdentity
            var claimIdentity = new ClaimsIdentity(new[] { claims }, "serverAuth");
            // create claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
            // login user
            await HttpContext.SignInAsync(claimsPrincipal);

            return Ok(new UserDisplayInfo(nickname, Status.Online));
        }

        [HttpGet("get")]
        public async Task<ActionResult<UserDisplayInfo>> GetCurrentUser()
        {
            UserDisplayInfo currentUser = new UserDisplayInfo();
            if (User.Identity.IsAuthenticated) {
                var nick = User.FindFirstValue(ClaimTypes.Name);
                var dbUser = await _context.Users.FirstOrDefaultAsync(user => user.Nickname == nick);
                currentUser = new UserDisplayInfo(dbUser.Nickname, Status.Online);
            }
            return Ok(currentUser);
        }

        [HttpGet("logout")]
        public async Task<ActionResult> LogoutUser()
        {
            if (User.Identity.IsAuthenticated) {
                var nick = User.FindFirstValue(ClaimTypes.Name);
                var dbUser = await _context.Users.FirstOrDefaultAsync(user => user.Nickname == nick);
                dbUser.IsOnline = false;
                _context.Users.Update(dbUser);
                await _context.SaveChangesAsync();
                await HttpContext.SignOutAsync();
            }
                
            return Ok();
        }
    }
}
