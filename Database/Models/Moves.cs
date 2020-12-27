using Microsoft.EntityFrameworkCore;
using POT.Pexeso.Shared;
using POT.Pexeso.Shared.Pexeso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Models
{
    public class Moves
    {
        public int Id { get; set; }
        public ICollection<MoveRecord> MoveRecords { get; set; }
    }
}
