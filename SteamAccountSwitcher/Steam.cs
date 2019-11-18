using System;
using System.Diagnostics;
using System.Linq;

namespace SteamAccountSwitcher
{
    internal class Steam
    {
        public Steam(string steamPath)
        {
            SteamPath = steamPath;
        }

        private string SteamPath { get; }

        private static bool IsSteamRunning()
        {
            return Process.GetProcessesByName("steam").Any();
        }

        private static void KillSteam()
        {
            foreach (var p in Process.GetProcessesByName("steam"))
                p.Kill();
        }

        public void StartSteamAccount(SteamAccount a)
        {
            var counter = 0;
            while (IsSteamRunning())
            {
                if (counter > 10)
                    throw new Exception("Could not stop Steam");
                counter++;
                KillSteam();
            }

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(SteamPath, a.GetStartParameters());
            p.Start();
        }

        public void LogoutSteam()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(SteamPath, "-shutdown");
            p.Start();
        }
    }
}