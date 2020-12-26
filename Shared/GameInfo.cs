using POT.Pexeso.Shared.Pexeso;
using System.Collections.Generic;

namespace POT.Pexeso.Shared
{
    public enum GameState
    {
        Starting,
        Running,
        CardShowDelay,
        Gameover,
        Ended
    }

    public class GameInfo
    {
        public Player Challenger { get; set; }
        public Player Opponent { get; set; }
        public int Connected { get; set; }
        public int Time { get; set; }
        public int Matches { get; set; }
        public Card LastFliped { get; set; }
        public bool IsTimeout { get; set; }
        public Player OnTheMove { get; set; }
        public GameState GameState { get; set; }
        public GameSettings GameSettings { get; set; }
        public Board Board { get; set; }
        public List<MoveRecord> Moves { get; set; }
    }
}
