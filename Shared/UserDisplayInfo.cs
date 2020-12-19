namespace POT.Pexeso.Shared
{
    public enum Status
    {
        InGame,
        Online
    }

    public class UserDisplayInfo
    {
        public string Nickname { get; set; }
        public Status Status { get; set; }

        public UserDisplayInfo() {}

        public UserDisplayInfo(string nickname, Status status)
        {
            Nickname = nickname;
            Status = status;
        }
    }
}
