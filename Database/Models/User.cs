namespace Database.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public int Loses { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; } 
        public bool IsOnline { get; set; }
    }
}
