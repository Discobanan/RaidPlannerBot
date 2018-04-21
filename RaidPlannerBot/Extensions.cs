using System;

namespace RaidPlannerBot
{
    public static class Extensions
    {
        public static T Log<T>(this T stuff, bool onlyIfDebug = false)
        {
            bool allowDebugOutput = AppConfig.Shared?.Debug ?? true;

            if ((allowDebugOutput && onlyIfDebug) || !onlyIfDebug)
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.Out.WriteLine($"[{timestamp}] {stuff}");
            }

            return stuff;
        }
    }
}
