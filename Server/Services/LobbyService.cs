using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Data;
using POT.Pexeso.Server.Hubs;
using POT.Pexeso.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace POT.Pexeso.Server.Services
{
    public class Details
    {
        public string ConnectionId { get; set; }
        public Status Status { get; set; }
        public int TimeElapsed { get; set; }
    }

    public class LobbyService
    {
        private ConcurrentDictionary<string, Details> _onlineUsers;
        private IHubContext<LobbyHub> _hub;

        private Timer _pairingTimer;

        public LobbyService(IHubContext<LobbyHub> hubContext)
        {
            _hub = hubContext;
            _onlineUsers = new ConcurrentDictionary<string, Details>();

            _pairingTimer = new Timer(1000);
            _pairingTimer.Elapsed += async (sender, e) => {
                var users = _onlineUsers.Where(pair => pair.Value.Status == Status.Pairing);
                foreach (var userPair in users) {
                    if (userPair.Value.TimeElapsed <= 0) {

                        //userPair.Value.Status = Status.Online;
                        await UnassignPairingAsync(new[] { userPair.Key });
                        await _hub.Clients.Client(userPair.Value.ConnectionId).SendAsync("InvitationCancelled", false);
                    }

                    await _hub.Clients.Client(userPair.Value.ConnectionId).SendAsync("TimerChanged", userPair.Value.TimeElapsed--);
                }
            };
            _pairingTimer.Start();
        }

        public void ConnectUser(string nickname, string connectioId, Status status)
        {
            _onlineUsers.TryAdd(nickname, new Details { ConnectionId = connectioId, Status = status });
        }

        public void DisconnectUser(string nickname)
        {
            _onlineUsers.Remove(nickname, out _);
        }

        public Dictionary<string, Details> GetAllOnlineUsers()
        {
            return _onlineUsers.ToDictionary(pair => pair.Key,
                                             pair => pair.Value);
        }

        public string GetConnectionId(string nick)
        {
            _onlineUsers.TryGetValue(nick, out var details);
            return details?.ConnectionId;
        }

        public async Task AssignForPairingAsync(IEnumerable<string> nicks)
        {
            foreach (var nick in nicks) {
                _onlineUsers[nick].TimeElapsed = GameSettings.ResponseDelay;
                _onlineUsers[nick].Status = Status.Pairing;

                await _hub.Clients.All.SendAsync("UserStatusChanged", nick, Status.Pairing);
            }
        }

        public async Task UnassignPairingAsync(IEnumerable<string> nicks)
        {
            foreach (var nick in nicks) {
                _onlineUsers[nick].Status = Status.Online;

                await _hub.Clients.All.SendAsync("UserStatusChanged", nick, Status.Online);
            }
        }
    }
}
