using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using POT.Pexeso.Data;
using POT.Pexeso.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private IHttpContextAccessor _httpContextAccessor;
        private PexesoDbContext _dataContext;
        private LobbyService _lobbyService;
        private GameService _gameService;

        public GameHub(IHttpContextAccessor httpContext, PexesoDbContext dataContext, LobbyService userService, GameService gameService)
        {
            _httpContextAccessor = httpContext;
            _dataContext = dataContext;
            _lobbyService = userService;
            _gameService = gameService;
        }

        public override async Task OnConnectedAsync()
        {
            var nick = GetNick();
            await _gameService.JoinToGame(nick, Context.ConnectionId);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var nick = GetNick();
            await _gameService.LeaveGame(nick);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CardFliped(int height, int width)
        {
            var nick = GetNick();
            await _gameService.FlipCard(nick, height, width);
        }

        private string GetNick()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }
    }
}
