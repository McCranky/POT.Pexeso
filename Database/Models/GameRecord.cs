using POT.Pexeso.Shared;
using POT.Pexeso.Shared.Pexeso;
using System;
using System.Collections.Generic;

namespace Database.Models
{
    public class GameRecord
    {
        public int Id { get; set; }
        public BoardSize BoardSize { get; set; }
        public int CardId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public string Winner { get; set; }
        public string Challenger { get; set; }
        public string Opponent { get; set; }
        public int ChallengerMoves { get; set; }
        public int OpponentMoves { get; set; }
        public ICollection<MoveRecord> Moves { get; set; }
    }
}
