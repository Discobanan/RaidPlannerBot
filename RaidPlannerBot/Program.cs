using System;

namespace RaidPlannerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = args.Length > 0 ? args[0] : "AppConfig.json";
            
            if (AppConfig.Load(configFile))
            {
                var bot = new Bot();
                bot.Start();
            }

			"Press any key to terminate process...".Log();
			Console.ReadKey(true);
		}
    }
}
