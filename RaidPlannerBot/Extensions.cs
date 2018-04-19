using System;

namespace RaidPlannerBot
{
    public static class Extensions
    {
        public static T Log<T>(this T stuff)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.Out.WriteLine($"[{timestamp}] {stuff}");
            return stuff;
        }
    }
}
