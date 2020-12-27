using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POT.Pexeso.Shared.Pexeso
{
    //[Keyless]
    public class MoveRecord
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public DateTime Time { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
