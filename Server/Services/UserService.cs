using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Data;
using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Services
{
    public class Details
    {
        public string ConnectionId { get; set; }
        public Status Status { get; set; }
    }

    public class UserService
    {
        private Dictionary<string, Details> _onlineUsers;

        public UserService()
        {
            _onlineUsers = new Dictionary<string, Details>();
        }

        public void ConnectUser(string nickname, string connectioId, Status status)
        {
            _onlineUsers.Add(nickname, new Details { ConnectionId = connectioId, Status = status });
        }

        public void DisconnectUser(string nickname)
        {
            _onlineUsers.Remove(nickname);
        }

        public Dictionary<string, Details> GetAllOnlineUsers()
        {
            return _onlineUsers;
        }

        public string GetConnectionId(string nick)
        {
            return _onlineUsers[nick].ConnectionId;
        }
    }
}
