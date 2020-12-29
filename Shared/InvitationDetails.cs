namespace POT.Pexeso.Shared
{
    public class InvitationDetails
    {
        public string NicknameTo { get; set; }
        public string NicknameFrom { get; set; }
        public GameSettings GameSettings { get; set; }
        public bool Rejected { get; set; }
    }
}
