using System;

namespace KeybindsPlus.Helpers
{
    public static class LogHelper
    {
        private static string FormatMessage(string message) => $"[{Constants.PluginName}] {message}";
        public static void LogError(Exception? ex, string message) => Plugin.Log.Error(ex, FormatMessage(message));
        public static void LogError(string message) => LogError(null, message);
        public static void LogWarning(string message) => Plugin.Log.Warning(FormatMessage(message));
        public static void LogInfo(string message) => Plugin.Log.Info(FormatMessage(message));
        public static void LogDebug(string message) => Plugin.Log.Debug(FormatMessage(message));

        public static void PrintError(string message) => Plugin.ChatGui.PrintError(FormatMessage(message));
        public static void Print(string message) => Plugin.ChatGui.Print(FormatMessage(message));
        public static void PrintUnavailable() => Plugin.ChatGui.PrintError(FormatMessage("That action is unavailable at this time."));
    }
}
