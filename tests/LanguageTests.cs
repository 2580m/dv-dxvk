using System;
using System.IO;
using DVVulkan;

internal static class LanguageTests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static int Main(string[] args)
    {
        string mod = args[0];
        Assert(Text.FromGameLanguage("zh-CN", "Chinese (Simplified)") == "zh", "Simplified Chinese not detected");
        Assert(Text.FromGameLanguage("zh_TW", "Chinese (Traditional)") == "zh", "Traditional Chinese not detected");
        Assert(Text.FromGameLanguage("", "简体中文") == "zh", "Chinese name fallback failed");
        Assert(Text.FromGameLanguage("", "Chinese") == "zh", "English Chinese name fallback failed");
        Assert(Text.FromGameLanguage("en", "English") == "en", "English detection failed");
        Assert(Text.FromGameLanguage("de", "German") == "en", "Unsupported language must fall back to English");
        Assert(Text.FromGameLanguage(null, null) == "en", "Uninitialized language fallback failed");

        Text.Language = "en";
        var error = new ModException("ForeignDll", "dxgi.dll");
        Assert(Text.ErrorMessage(error).Contains("Foreign DLL conflict"), "English conflict message missing");
        Text.Language = "zh";
        Assert(Text.ErrorMessage(error).Contains("其他来源"), "Existing error did not follow a language switch");
        Assert(Text.Get("UninstallButton").Contains("卸载"), "Chinese uninstall button missing");
        Assert(Text.Get("Pending", Text.Get("enable")).Contains("启用"), "Pending action was not translated");
        Text.WriteError(mod, error);
        Assert(File.ReadAllText(Path.Combine(mod, "deployment.log")).Contains("错误："), "Chinese on-disk error log missing");
        Text.Language = "en";
        Assert(Text.ReadLog(mod).Contains("ERROR: Foreign DLL conflict"), "Error log view did not follow English switch");
        Text.WriteLog(mod, "AppliedUninstall", "", false);
        Assert(File.ReadAllText(Path.Combine(mod, "deployment.log")).Contains("Applied: uninstall"), "English success log missing");
        Text.Language = "zh";
        Assert(Text.ReadLog(mod).Contains("已应用：卸载"), "Success log view did not follow Chinese switch");

        File.WriteAllText(Path.Combine(mod, "language.txt"), "zh");
        Text.LoadLanguage(mod);
        Assert(Text.Language == "zh", "Worker language persistence failed");
        File.WriteAllText(Path.Combine(mod, "language.txt"), "invalid");
        Text.LoadLanguage(mod);
        Assert(Text.Language == "en", "Invalid worker language fallback failed");
        Console.WriteLine("PASS: game language mapping, UI text, status/error retranslation, saved language, localized disk logs and dynamic log view.");
        return 0;
    }
}
