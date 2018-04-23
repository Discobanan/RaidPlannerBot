using System;
using System.IO;
using Newtonsoft.Json;

namespace RaidPlannerBot
{
    public class AppConfig
    {
        [JsonProperty("planExpirationMinutes")]
        public int PlanExpirationMinutes { get; set; }

        [JsonProperty("planPersisentStorageLocation")]
        public string PlanPersisentStorageLocation { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("discordBotToken")]
        public string DiscordBotToken { get; set; }

        [JsonProperty("discordPlaying")]
        public string DiscordPlaying { get; set; }

        [JsonProperty("logFile")]
        public string LogFile { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        public static AppConfig Shared;

        public static bool Load(string configFile)
        {
            if (!File.Exists(configFile))
            {
                $"Config file {configFile} not found! Open {System.IO.Directory.GetCurrentDirectory()}, rename AppConfig.example.json to AppConfig.json, and edit it to match your desired configuration.".Log();
                return false;
            }

            AppConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configFile));
            }
            catch (Exception e)
            {
                $"Error when loading config file {configFile}: {e.Message}".Log();
                return false;
            }

            try
            {
                if (!Directory.Exists(config.PlanPersisentStorageLocation))
                    Directory.CreateDirectory(config.PlanPersisentStorageLocation);
            }
            catch (Exception e)
            {
                $"Error when creating persistant storage location at {config.PlanPersisentStorageLocation}: {e.Message}".Log();
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.DiscordBotToken))
            {
                $"Config-file is missing the discord token!".Log();
                return false;
            }

            Shared = config;
            return true;
        }
    }
}
