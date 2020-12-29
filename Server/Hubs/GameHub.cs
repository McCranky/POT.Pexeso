using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using POT.Pexeso.Server.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LobbyService _lobbyService;
        private readonly GameService _gameService;

        public GameHub(IHttpContextAccessor httpContext, LobbyService userService, GameService gameService)
        {
            _httpContextAccessor = httpContext;
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
            _gameService.LeaveGame(nick);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CardFliped(int height, int width)
        {
            var nick = GetNick();
            await _gameService.FlipCard(nick, height, width);
        }

        public async Task SendMessage(string text, string from, string to)
        {
            var connId = _lobbyService.GetConnectionId(to);
            await Clients.Client(connId).SendAsync("ReceiveMessage", text, from);
            //await _gameService.FlipCard(nick, height, width);
        }

        private string GetNick()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        }
    }
}
