using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public int Loses { get; set; }
        public int Wins { get; set; }
        public bool IsOnline { get; set; }
    }
}
