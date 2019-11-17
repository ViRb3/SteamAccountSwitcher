namespace SteamAccountSwitcher
{
    public class SteamAccount
    {
        public SteamAccount()
        {
        }

        public SteamAccount(string username, string password)
        {
            Name = username;
            Username = username;
            Password = password;
            Type = AccountType.Main;
        }

        public string Name { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public AccountType Type { get; set; }

        public string Icon
        {
            get
            {
                if (Type == AccountType.Main) return "steam-ico-main.png";
                if (Type == AccountType.Smurf) return "steam-ico-smurf.png";
                return null;
            }
        }

        public string getStartParameters()
        {
            return "-login " + Username + " " + Password;
        }

        public override string ToString()
        {
            return Name + "~ (user: " + Username + ")";
        }
    }
}