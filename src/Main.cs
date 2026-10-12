using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityModManagerNet;
using I2.Loc;

namespace DVVulkan
{
    public static class Main
    {
        private static string mod;
        private static string root;
        private static string statusKey;
        private static Exception statusError;
        private static string runtimeKey;
        private static Exception runtimeError;
        private static string savedLanguage;
        private static float languageTimer;

        public static bool Load(UnityModManager.ModEntry entry)
        {
            mod = Path.GetFullPath(entry.Path);
            root = Path.GetFullPath(Path.Combine(mod, "..", ".."));
            Text.LoadLanguage(mod);
            RefreshLanguage(entry);
            DetectRuntime();
            entry.OnToggle = Toggle;
            entry.OnGUI = Draw;
            entry.OnUpdate = delegate(UnityModManager.ModEntry current, float delta) {
                languageTimer += delta;
                if (languageTimer >= 1f) { languageTimer = 0f; RefreshLanguage(current); }
            };
            entry.OnSaveGUI = RefreshLanguage;
            return true;
        }

        private static void RefreshLanguage(UnityModManager.ModEntry entry)
        {
            // Read the game's selected language, not the operating system locale.
            try { Text.Language = Text.FromGameLanguage(LocalizationManager.CurrentLanguageCode, LocalizationManager.CurrentLanguage); }
            catch { return; } // Preserve the last known language while localization initializes.
            if (savedLanguage == Text.Language) return;
            try
            {
                File.WriteAllText(Path.Combine(mod, "language.txt"), Text.Language);
                savedLanguage = Text.Language;
            }
            catch (Exception e) { entry.Logger.Error(Text.Get("LanguageSaveFailed", Text.ErrorMessage(e))); }
        }

        private static void SetStatus(UnityModManager.ModEntry entry, string key, Exception error = null)
        {
            statusKey = key;
            statusError = error;
            string message = Text.Get(key, error == null ? "" : Text.ErrorMessage(error));
            if (error == null) entry.Logger.Log(message);
            else entry.Logger.Error(message);
        }

        private static bool Toggle(UnityModManager.ModEntry entry, bool enable)
        {
            // An uninstall request survives a restart or a stale UMM enabled setting.
            string action = File.Exists(Path.Combine(mod, "uninstalled.txt")) ? "uninstall" : (enable ? "enable" : "disable");
            return Queue(entry, action);
        }

        private static bool Queue(UnityModManager.ModEntry entry, string action)
        {
            RefreshLanguage(entry);
            string pending = Path.Combine(mod, "pending.txt");
            string previous = null;
            bool requestWritten = false;
            try
            {
                Deployment.Validate(root, mod);
                previous = File.Exists(pending) ? File.ReadAllText(pending) : null;
                File.WriteAllText(pending, action);
                requestWritten = true;
                using (var game = Process.GetCurrentProcess())
                {
                    var start = new ProcessStartInfo(Path.Combine(mod, "DVVulkan.Worker.exe"));
                    start.Arguments = "\"" + root + "\" " + game.Id + " " + game.StartTime.ToUniversalTime().Ticks;
                    start.UseShellExecute = false;
                    start.CreateNoWindow = true;
                    start.WindowStyle = ProcessWindowStyle.Hidden;
                    start.WorkingDirectory = mod;
                    using (var worker = Process.Start(start))
                    {
                        if (worker == null) throw new ModException("WorkerStartFailed");
                    }
                }
                SetStatus(entry, action == "enable" ? "QueuedEnable" : action == "uninstall" ? "QueuedUninstall" : "QueuedDisable");
                return true;
            }
            catch (Exception e)
            {
                if (requestWritten)
                {
                    try
                    {
                        if (previous == null) File.Delete(pending);
                        else File.WriteAllText(pending, previous);
                    }
                    catch (Exception restoreError) { entry.Logger.Error(Text.Get("RestorePendingFailed", Text.ErrorMessage(restoreError))); }
                }
                SetStatus(entry, "SwitchFailed", e);
                return false;
            }
        }

        private static void Uninstall(UnityModManager.ModEntry entry)
        {
            string marker = Path.Combine(mod, "uninstalled.txt");
            bool existed = File.Exists(marker);
            try
            {
                // Set this first so neither a later toggle nor the next startup redeploys DXVK.
                RefreshLanguage(entry);
                File.WriteAllText(marker, Text.Get("MarkerRequested"));
                if (!Queue(entry, "uninstall"))
                {
                    if (!existed) File.Delete(marker);
                    return;
                }
                entry.Active = false;
                entry.Enabled = false;
            }
            catch (Exception e)
            {
                SetStatus(entry, "UninstallFailed", e);
            }
        }

        private static void Install(UnityModManager.ModEntry entry)
        {
            string marker = Path.Combine(mod, "uninstalled.txt");
            try
            {
                // Queue while the marker still exists; workers cannot apply until this game exits.
                if (!Queue(entry, "enable")) return;
                File.Delete(marker);
                entry.Enabled = true;
                entry.Active = true;
            }
            catch (Exception e)
            {
                SetStatus(entry, "InstallFailed", e);
            }
        }

        private static void DetectRuntime()
        {
            try
            {
                int count = 0;
                using (var game = Process.GetCurrentProcess())
                    foreach (ProcessModule module in game.Modules)
                        for (int i = 0; i < Deployment.Names.Length; i++)
                            if (string.Equals(module.FileName, Path.Combine(root, Deployment.Names[i]), StringComparison.OrdinalIgnoreCase)
                                && Deployment.Matches(module.FileName, i)) count++;
                runtimeKey = count == 2 ? "RuntimeLoaded" : "RuntimeMissing";
            }
            catch (Exception e) { runtimeKey = "RuntimeFailed"; runtimeError = e; }
        }

        private static void Draw(UnityModManager.ModEntry entry)
        {
            RefreshLanguage(entry);
            GUILayout.Label("DV Vulkan / DXVK 3.1.1 (Windows x64)");
            GUILayout.Label(Text.Get(runtimeKey, runtimeError == null ? "" : Text.ErrorMessage(runtimeError)));
            GUILayout.Label(Text.Get("Renderer", SystemInfo.graphicsDeviceType, SystemInfo.graphicsDeviceName));
            GUILayout.Label(Text.Get("ToggleHelp"));
            GUILayout.Label(Text.Get("ApiHelp"));
            GUILayout.Label(Text.Get("UninstallHelp"));
            if (GUILayout.Button(Text.Get("UninstallButton"))) Uninstall(entry);
            if (File.Exists(Path.Combine(mod, "uninstalled.txt")))
            {
                GUILayout.Label(Text.Get("UninstalledHelp"));
                if (GUILayout.Button(Text.Get("InstallButton"))) Install(entry);
            }
            if (!string.IsNullOrEmpty(statusKey)) GUILayout.Label(Text.Get(statusKey, statusError == null ? "" : Text.ErrorMessage(statusError)));
            string pending = Path.Combine(mod, "pending.txt");
            if (File.Exists(pending)) GUILayout.Label(Text.Get("Pending", Text.Get(File.ReadAllText(pending).Trim())));
            try
            {
                string log = Text.ReadLog(mod);
                if (log != null) GUILayout.TextArea(log);
            }
            catch (Exception e) { GUILayout.Label(Text.Get("LogReadFailed", Text.ErrorMessage(e))); }
        }
    }
}
