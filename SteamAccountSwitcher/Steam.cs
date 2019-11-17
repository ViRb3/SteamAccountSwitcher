using System.Diagnostics;
using System.IO;

namespace SteamAccountSwitcher
{
    internal class Steam
    {
        public Steam(string installDir)
        {
            InstallDir = installDir;
        }

        public string InstallDir { get; set; }

        public bool IsSteamRunning()
        {
            var pname = Process.GetProcessesByName("steam");
            if (pname.Length == 0)
                return false;
            return true;
        }

        public void KillSteam()
        {
            var proc = Process.GetProcessesByName("steam");
            proc[0].Kill();
        }

        public bool StartSteamAccount(SteamAccount a)
        {
            var finished = false;

            if (IsSteamRunning()) KillSteam();

            while (finished == false)
                if (IsSteamRunning() == false)
                {
                    var p = new Process();
                    if (File.Exists(InstallDir))
                    {
                        p.StartInfo = new ProcessStartInfo(InstallDir, a.GetStartParameters());
                        p.Start();
                        finished = true;
                        return true;
                    }
                }

            return false;
        }


        public bool LogoutSteam()
        {
            var p = new Process();
            if (File.Exists(InstallDir))
            {
                p.StartInfo = new ProcessStartInfo(InstallDir, "-shutdown");
                p.Start();
                return true;
            }

            return false;
        }
    }
}