using System.Collections.Generic;

namespace SteamAccountSwitcher
{
    public class SteamData
    {
        public SteamData()
        {
            Accounts = new List<SteamAccount>();
        }

        public string SteamFilePath { get; set; }
        public List<SteamAccount> Accounts { get; }
    }
}