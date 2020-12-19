using Microsoft.AspNetCore.Mvc;
using POT.Pexeso.Server.Services;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAllOnlineUsers()
                .Select(pair => new UserDisplayInfo { Nickname = pair.Key, Status = pair.Value.Status }).ToList();
            return Ok(users);
        }
    }
}
