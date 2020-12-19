using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Data;
using POT.Pexeso.Server.Services;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        private IHttpContextAccessor _httpContextAccessor;
        private PexesoDataContext _dataContext;
        private UserService _userService;
        public LobbyHub(IHttpContextAccessor httpContext, PexesoDataContext dataContext, UserService userService)
        {
            _httpContextAccessor = httpContext;
            _dataContext = dataContext;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            // ziskam nick usera
            var nick = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"{nick} connected.");
            // zaradim medzi prihlasenych
            _userService.ConnectUser(nick, Context.ConnectionId, Shared.Status.Online);
            // prihlasim ho v databaze
            var dbUser = await _dataContext.Users.FirstOrDefaultAsync(user => user.Nickname == nick);
            dbUser.IsOnline = true;
            _dataContext.Users.Update(dbUser);
            await _dataContext.SaveChangesAsync();
            // poslem mu aktualny zoznam
            await Clients.Caller.SendAsync("GetAllUsers", _userService.GetAllOnlineUsers().Select(pair => pair.Key).Where(name => name != nick));
            // ostatnym poslem noveho usera
            await Clients.Others.SendAsync("UserJoined", nick);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // ziskam nick
            var nick = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"{nick} disconnected.");
            // zmazem z prihlasenych
            _userService.DisconnectUser(nick);
            // odhlasim v databaze
            var dbUser = await _dataContext.Users.FirstOrDefaultAsync(user => user.Nickname == nick);
            dbUser.IsOnline = false;
            _dataContext.Users.Update(dbUser);
            await _dataContext.SaveChangesAsync();
            // poslem ostatnym aktualny zoznam odhlasenych
            await Clients.Others.SendAsync("UserLeft", nick);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendInvitation(InvitationDetails details)
        {
            var nick = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            details.NicknameFrom = nick;

            var connectionId = _userService.GetConnectionId(details.NicknameTo);
            await Clients.Client(connectionId).SendAsync("ReceiveInvitation", details);
        }
    }
}
