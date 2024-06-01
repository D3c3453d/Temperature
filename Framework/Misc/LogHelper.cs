using StardewModdingAPI;

namespace Temperature.Framework.Misc
{
    public static class LogHelper
    {
        private static readonly IMonitor monitor = ModEntry.Instance.Monitor;
        public static void Verbose(string str)
        {
            monitor.VerboseLog(str);
        }

        public static void Trace(string str)
        {
            monitor.Log(str, LogLevel.Trace);
        }

        public static void Debug(string str)
        {
            monitor.Log(str, LogLevel.Debug);
        }

        public static void Info(string str)
        {
            monitor.Log(str, LogLevel.Info);
        }

        public static void Warn(string str)
        {
            monitor.Log(str, LogLevel.Warn);
        }

        public static void Error(string str)
        {
            monitor.Log(str, LogLevel.Error);
        }
    }
}