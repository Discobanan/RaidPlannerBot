using System;
using System.IO;

namespace RaidPlannerBot
{
    public static class Extensions
    {
        public static T Log<T>(this T stuff, bool onlyIfDebug = false)
        {
            bool allowDebugOutput = AppConfig.Shared?.Debug ?? true;
            string logFile = AppConfig.Shared?.LogFile ?? string.Empty;

            if ((allowDebugOutput && onlyIfDebug) || !onlyIfDebug)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var msg = $"[{timestamp}] {stuff}";

                Console.Out.WriteLine(msg);

                if (!string.IsNullOrWhiteSpace(logFile))
                    File.AppendAllText(logFile, $"{msg}\n");
            }

            return stuff;
        }
    }
}
