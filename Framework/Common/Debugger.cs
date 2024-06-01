using StardewModdingAPI;

namespace Temperature.Framework.Common
{
    public static class Debugger
    {
        private static readonly IMonitor monitor = ModEntry.Instance.Monitor;

        public static void Log(string message, string type)
        {
            switch (type)
            {
                case "Trace":
                    monitor.Log(message, LogLevel.Trace);
                    break;

                case "Info":
                    monitor.Log(message, LogLevel.Info);
                    break;

                case "Error":
                    monitor.Log(message, LogLevel.Error);
                    break;

                case "Warn":
                    monitor.Log(message, LogLevel.Warn);
                    break;

                case "Alert":
                    monitor.Log(message, LogLevel.Alert);
                    break;

                case "Debug":
                    monitor.Log(message, LogLevel.Debug);
                    break;
            }
        }
    }
}