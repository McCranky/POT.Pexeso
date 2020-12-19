using POT.Pexeso.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace POT.Pexeso.Server.Data.Models
{
    public class Card
    {
        public int Id { get; set; }
        public CardType Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Source { get; set; }
    }
}
