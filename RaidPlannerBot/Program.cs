using Newtonsoft.Json;
using System;
using System.IO;

namespace RaidPlannerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = LoadConfig(args);

            if (config != null)
            {
                var bot = new Bot(config);
                bot.Start();
            }

            "Press any key to terminate process...".Log();
            Console.ReadKey(true);
        }

        private static AppConfig LoadConfig(string[] args)
        {
            var configFile = args.Length > 0 ? args[0] : "AppConfig.json";
            if (!File.Exists(configFile))
            {
                $"Config file {configFile} not found!".Log();
                return null;
            }

            AppConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configFile));
            }
            catch (Exception e)
            {
                $"Error when loading config file {configFile}: {e.Message}".Log();
                return null;
            }

            try
            {
                if (!Directory.Exists(config.PlanPersisentStorageLocation))
                    Directory.CreateDirectory(config.PlanPersisentStorageLocation);
            }
            catch (Exception e)
            {
                $"Error when creating persistant storage location at {config.PlanPersisentStorageLocation}: {e.Message}".Log();
                return null;
            }

            if (string.IsNullOrWhiteSpace(config.DiscordBotToken))
            {
                $"Config-file is missing the discord token!".Log();
                return null;
            }

            return config;
        }
    }
}
