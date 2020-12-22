﻿using Microsoft.AspNetCore.Authorization;
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
using System.Timers;

namespace POT.Pexeso.Server.Hubs
{
    [Authorize]
    public class LobbyHub : Hub
    {
        private IHttpContextAccessor _httpContextAccessor;
        private PexesoDataContext _dataContext;
        private LobbyService _lobbyService;

        //private bool _isCancelled;
        //private Timer _waitingTimer;
        //private InvitationDetails _details;
        private int _counter;

        public LobbyHub(IHttpContextAccessor httpContext, PexesoDataContext dataContext, LobbyService userService)
        {
            _httpContextAccessor = httpContext;
            _dataContext = dataContext;
            _lobbyService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            // ziskam nick usera
            var nick = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Console.WriteLine($"{nick} connected.");
            // zaradim medzi prihlasenych
            _lobbyService.ConnectUser(nick, Context.ConnectionId, Shared.Status.Online);
            // prihlasim ho v databaze
            var dbUser = await _dataContext.Users.FirstOrDefaultAsync(user => user.Nickname == nick);
            dbUser.IsOnline = true;
            _dataContext.Users.Update(dbUser);
            await _dataContext.SaveChangesAsync();
            // poslem mu aktualny zoznam
            await Clients.Caller.SendAsync("GetAllUsers", _lobbyService.GetAllOnlineUsers()
                .Select(pair => new UserDisplayInfo { 
                    Nickname = pair.Key, 
                    Status = pair.Value.Status 
                })
                .Where(info => info.Nickname != nick));
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
            _lobbyService.DisconnectUser(nick);
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
            var connectionId = _lobbyService.GetConnectionId(details.NicknameTo);

            await Clients.Client(connectionId).SendAsync("ReceiveInvitation", details);

            await _lobbyService.AssignForPairingAsync(new[] { details.NicknameFrom, details.NicknameTo });
        }

        //private async void WaitForResponse(Object source, ElapsedEventArgs e)
        //{
        //    var ids = new string[2];
        //    ids[0] = _lobbyService.GetConnectionId(_details.NicknameFrom);
        //    ids[1] = _lobbyService.GetConnectionId(_details.NicknameTo);

        //    await Clients.Clients(ids).SendAsync("TimerChanged", _counter);

        //    if (_counter-- < 0) {
        //        await CancelInvitation(_details);
        //    }
        //}

        public async Task CancelInvitation(InvitationDetails details)
        {
            var ids = new List<string>();
            var nicks = new List<string>();

            ids.Add(_lobbyService.GetConnectionId(details.NicknameTo));
            nicks.Add(details.NicknameTo);
            if (!string.IsNullOrWhiteSpace(details.NicknameFrom)) {
                ids.Add(_lobbyService.GetConnectionId(details.NicknameFrom));
                nicks.Add(details.NicknameFrom);
            } else {
                var nick = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                nicks.Add(nick);
            }

            await _lobbyService.UnassignPairingAsync(nicks);

            await Clients.Clients(ids).SendAsync("InvitationCancelled", details.Rejected);
        }

        public async Task AcceptInvitation(InvitationDetails details)
        {

        }
    }
}
