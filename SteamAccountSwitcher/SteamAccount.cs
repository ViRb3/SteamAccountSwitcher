namespace SteamAccountSwitcher
{
    public class SteamAccount
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public AccountType Type { get; set; }

        public string GetStartParameters()
        {
            return "-login " + Username + " " + Password;
        }

        public override string ToString()
        {
            return Name + "~ (user: " + Username + ")";
        }
    }
}