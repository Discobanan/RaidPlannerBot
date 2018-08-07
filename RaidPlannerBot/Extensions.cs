using System;
using System.IO;

namespace RaidPlannerBot
{
    public static class Extensions
    {
        private static Object logLock = new Object();

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
                {
                    lock (logLock)
                    {
                        File.AppendAllText(logFile, $"{msg}\n");
                    }
                }
            }

            return stuff;
        }

		public static string Capitalize(this string str)
		{
			return str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower();
		}

	}
}
