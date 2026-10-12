using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DVVulkan
{
    // Shared by the game mod and the standalone deployment worker.
    public static class Text
    {
        public static string Language = "en";
        private static readonly Dictionary<string, string[]> Messages = new Dictionary<string, string[]> {
            { "Renderer", new[] { "Unity rendering interface: {0} | {1}", "Unity 渲染接口：{0} | {1}" } },
            { "ToggleHelp", new[] { "Use the UMM enable switch. Changes apply after exiting the game and take effect next launch, including the first activation.", "使用 UMM 启用开关。切换在退出游戏后应用，下次启动生效；首次启用也需重启。" } },
            { "ApiHelp", new[] { "Unity still reports Direct3D11 with DXVK. Do not use -force-vulkan or -force-d3d12.", "DXVK 下 Unity 仍显示 Direct3D11，这是正常的。请勿使用 -force-vulkan 或 -force-d3d12。" } },
            { "UninstallHelp", new[] { "Uninstall removes the root DXVK DLLs and disables the mod after you exit. Wait for completion before removing the mod with UMM.", "卸载会在退出游戏后移除游戏根目录的 DXVK DLL 并停用模组。请等待完成后再用 UMM 删除模组。" } },
            { "UninstallButton", new[] { "Uninstall / Remove DXVK and disable", "卸载 / 移除 DXVK 并停用" } },
            { "InstallButton", new[] { "Install / Restore DXVK", "安装 / 恢复 DXVK" } },
            { "UninstalledHelp", new[] { "DXVK removal was requested. The UMM switch will not redeploy it. After removal completes, UMM can delete the mod files.", "已请求移除 DXVK，普通启用开关不会重新部署。移除完成后可用 UMM 删除模组文件。" } },
            { "RuntimeLoaded", new[] { "This process loaded the DXVK 3.1.1 DLLs. See DXVK logs for Vulkan device details.", "本次进程已加载 DXVK 3.1.1 转译 DLL；Vulkan 设备详情请查看 DXVK 日志。" } },
            { "RuntimeMissing", new[] { "This process did not load both DXVK DLLs.", "本次进程未加载完整 DXVK 转译 DLL。" } },
            { "RuntimeFailed", new[] { "Unable to inspect rendering DLLs: {0}", "无法读取本次渲染 DLL：{0}" } },
            { "QueuedEnable", new[] { "Enable queued. Exit the game and wait for DLL deployment before launching again.", "已排队启用：退出游戏，等待 DLL 部署完成后再启动。" } },
            { "QueuedDisable", new[] { "Disable queued. Exit the game to restore native Direct3D 11 next launch.", "已排队停用：退出游戏后恢复原生 Direct3D 11，下次启动生效。" } },
            { "QueuedUninstall", new[] { "Uninstall queued. Exit the game to remove the root DXVK DLLs. After the log confirms completion, remove the mod with UMM.", "已排队卸载：退出游戏后移除根目录 DXVK DLL；日志确认完成后可用 UMM 删除模组。" } },
            { "SwitchFailed", new[] { "Switch failed: {0}", "切换失败：{0}" } },
            { "UninstallFailed", new[] { "Uninstall request failed: {0}", "卸载请求失败：{0}" } },
            { "InstallFailed", new[] { "Restore request failed: {0}", "恢复请求失败：{0}" } },
            { "RestorePendingFailed", new[] { "Could not restore the previous pending request: {0}", "无法恢复之前的待执行请求：{0}" } },
            { "LanguageSaveFailed", new[] { "Could not save the game language for the deployment worker: {0}", "无法为后台部署程序保存游戏语言：{0}" } },
            { "Pending", new[] { "Pending: {0}", "待执行：{0}" } },
            { "enable", new[] { "Enable DXVK", "启用 DXVK" } },
            { "disable", new[] { "Disable DXVK", "停用 DXVK" } },
            { "uninstall", new[] { "Uninstall DXVK", "卸载 DXVK" } },
            { "AppliedEnable", new[] { "Applied: enable. DXVK DLLs deployed; effective next launch.", "已应用：启用。DXVK DLL 已部署，下次启动生效。" } },
            { "AppliedDisable", new[] { "Applied: disable. DXVK DLLs removed; native Direct3D 11 restored next launch.", "已应用：停用。DXVK DLL 已移除，下次启动恢复原生 Direct3D 11。" } },
            { "AppliedUninstall", new[] { "Applied: uninstall. DXVK DLLs removed; the mod files can now be removed with UMM.", "已应用：卸载。DXVK DLL 已移除，现在可用 UMM 删除模组文件。" } },
            { "MarkerRequested", new[] { "DXVK uninstall requested. Use the Install button to restore.", "已请求卸载 DXVK。需要恢复时使用安装按钮。" } },
            { "MarkerRemoved", new[] { "DXVK removed. Use the Install button to restore.", "DXVK 已移除。需要恢复时使用安装按钮。" } },
            { "NotGame", new[] { "Not a Derail Valley installation.", "此目录不是 Derail Valley 安装目录。" } },
            { "PayloadCorrupt", new[] { "DXVK payload is missing or corrupt: {0}", "DXVK 文件缺失或损坏：{0}" } },
            { "ForeignDll", new[] { "Foreign DLL conflict; no files changed: {0}", "存在其他来源的同名 DLL，未修改任何文件：{0}" } },
            { "WorkerStartFailed", new[] { "Could not start the deployment worker.", "无法启动后台部署程序。" } },
            { "GameRunning", new[] { "Close all Derail Valley instances, then run Apply-Pending.cmd.", "请关闭所有 Derail Valley 游戏进程，然后运行 Apply-Pending.cmd。" } },
            { "InvalidRequest", new[] { "Invalid pending request: {0}", "待执行请求无效：{0}" } },
            { "WorkerUsage", new[] { "Usage: DVVulkan.Worker.exe <game-directory> <PID or 0> <process-start-ticks or 0>", "用法：DVVulkan.Worker.exe <游戏目录> <进程 ID 或 0> <进程启动时间刻度或 0>" } },
            { "Error", new[] { "ERROR: {0}", "错误：{0}" } },
            { "SystemError", new[] { "System error ({0}, {1}): {2}", "系统错误（{0}，{1}）：{2}" } },
            { "SystemErrorDetail", new[] { "System error: {0}", "系统错误：{0}" } },
            { "LogReadFailed", new[] { "Unable to read deployment log: {0}", "无法读取部署日志：{0}" } }
        };

        public static string FromGameLanguage(string code, string name)
        {
            string c = (code ?? "").Trim().ToLowerInvariant().Replace('_', '-');
            string n = (name ?? "").Trim().ToLowerInvariant();
            return c == "zh" || c.StartsWith("zh-") || n.Contains("chinese") || n.Contains("中文") || n.Contains("汉语") || n.Contains("漢語") ? "zh" : "en";
        }

        public static string Get(string key, params object[] args)
        {
            string[] values;
            if (!Messages.TryGetValue(key, out values)) return key;
            return string.Format(values[Language == "zh" ? 1 : 0], args);
        }

        public static void LoadLanguage(string mod)
        {
            try { Language = File.ReadAllText(Path.Combine(mod, "language.txt")).Trim() == "zh" ? "zh" : "en"; }
            catch { Language = "en"; }
        }

        public static string ErrorMessage(Exception error)
        {
            var localized = error as ModException;
            if (localized != null) return Get(localized.Key, localized.Detail);
            // Preserve OS diagnostics and file paths as supplied by the system.
            return Get("SystemError", error.GetType().Name, "0x" + error.HResult.ToString("X8"), error.Message);
        }

        public static string SuccessKey(string action)
        {
            return action == "enable" ? "AppliedEnable" : action == "disable" ? "AppliedDisable" : "AppliedUninstall";
        }

        public static void WriteLog(string mod, string key, string detail, bool error)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // Keep language-neutral data so the in-game log view follows language changes too.
            File.WriteAllLines(Path.Combine(mod, "deployment.result"), new[] {
                timestamp, key, error ? "error" : "success", Convert.ToBase64String(Encoding.UTF8.GetBytes(detail ?? ""))
            }, Encoding.UTF8);
            File.WriteAllText(Path.Combine(mod, "deployment.log"), RenderLog(timestamp, key, detail, error) + Environment.NewLine, Encoding.UTF8);
        }

        private static string RenderLog(string timestamp, string key, string detail, bool error)
        {
            string message = Get(key, detail);
            if (error) message = Get("Error", message);
            return timestamp + " " + message;
        }

        public static string ReadLog(string mod)
        {
            string result = Path.Combine(mod, "deployment.result");
            if (File.Exists(result))
            {
                string[] data = File.ReadAllLines(result);
                if (data.Length == 4)
                    return RenderLog(data[0], data[1], Encoding.UTF8.GetString(Convert.FromBase64String(data[3])), data[2] == "error");
            }
            string log = Path.Combine(mod, "deployment.log");
            return File.Exists(log) ? File.ReadAllText(log) : null;
        }

        public static void WriteError(string mod, Exception error)
        {
            var localized = error as ModException;
            if (localized != null) WriteLog(mod, localized.Key, localized.Detail, true);
            else WriteLog(mod, "SystemErrorDetail", error.GetType().Name + " (0x" + error.HResult.ToString("X8") + "): " + error.Message, true);
        }
    }

    public sealed class ModException : IOException
    {
        public readonly string Key;
        public readonly string Detail;
        public ModException(string key, string detail = "") : base(Text.Get(key, detail)) { Key = key; Detail = detail; }
    }
}
