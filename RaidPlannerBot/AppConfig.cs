using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RaidPlannerBot
{
    public class AppConfig
    {
        [JsonProperty("planExpirationMinutes")]
        public int PlanExpirationMinutes { get; set; }

        [JsonProperty("planPersisentStorageLocation")]
        public string PlanPersisentStorageLocation { get; set; }

        [JsonProperty("discordBotToken")]
        public string DiscordBotToken { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }
    }
}
