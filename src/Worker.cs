using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DVVulkan
{
    internal static class Worker
    {
        private static int Main(string[] args)
        {
            string mod = AppDomain.CurrentDomain.BaseDirectory;
            Text.LoadLanguage(mod);
            try
            {
                if (args.Length != 3) throw new ModException("WorkerUsage");
                string root = Path.GetFullPath(args[0]);
                int pid = int.Parse(args[1]);
                long ticks = long.Parse(args[2]);
                // Serialize workers for this installation. All consume the latest request.
                string mutexName = "Local\\DVVulkan_" + root.ToUpperInvariant().GetHashCode().ToString("X8");
                using (var mutex = new Mutex(false, mutexName))
                {
                    try { mutex.WaitOne(); } catch (AbandonedMutexException) { }
                    try
                    {
                        if (pid != 0)
                        {
                            Process game = null;
                            try { game = Process.GetProcessById(pid); } catch (ArgumentException) { }
                            if (game != null)
                                using (game)
                                    if (game.StartTime.ToUniversalTime().Ticks == ticks) game.WaitForExit();
                        }
                        string pending = Path.Combine(mod, "pending.txt");
                        if (!File.Exists(pending)) return 0;
                        // Fail safely if another game instance was launched before deployment.
                        foreach (var game in Process.GetProcessesByName("DerailValley"))
                            using (game)
                                if (!game.HasExited) throw new ModException("GameRunning");
                        string action = File.ReadAllText(pending).Trim();
                        Text.LoadLanguage(mod);
                        if (action != "enable" && action != "disable" && action != "uninstall") throw new ModException("InvalidRequest", action);
                        // A durable uninstall marker overrides stale enable requests.
                        string marker = Path.Combine(mod, "uninstalled.txt");
                        if (File.Exists(marker)) action = "uninstall";
                        Deployment.Apply(root, mod, action == "enable");
                        if (action == "uninstall") File.WriteAllText(marker, Text.Get("MarkerRemoved"));
                        File.Delete(pending);
                        Text.WriteLog(mod, Text.SuccessKey(action), "", false);
                        return 0;
                    }
                    finally { mutex.ReleaseMutex(); }
                }
            }
            catch (Exception e)
            {
                Text.LoadLanguage(mod);
                Text.WriteError(mod, e);
                return 1;
            }
        }
    }
}
