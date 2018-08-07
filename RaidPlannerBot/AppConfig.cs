using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RaidPlannerBot
{
    public class AppConfig
    {
        [JsonProperty("planExpirationMinutes")]
        public int PlanExpirationMinutes { get; set; }

        [JsonProperty("exPlanExpirationDays")]
        public int ExPlanExpirationDays { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("discordBotToken")]
        public string DiscordBotToken { get; set; }

        [JsonProperty("discordPlaying")]
        public string DiscordPlaying { get; set; }

        [JsonProperty("persisentStorageLocation")]
        public string PersisentStorageLocation { get; set; }

        [JsonProperty("logFile")]
        public string LogFile { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("execOnDisconnectFilename")]
        public string ExecOnDisconnectFilename { get; set; }

        [JsonProperty("execOnDisconnectArguments")]
        public string ExecOnDisconnectArguments { get; set; }

		[JsonProperty("locations")]
		public List<Location> Locations { get; set; }


		public static AppConfig Shared;
		public static List<string> AlolanPrefixes = new List<string> { "a", "alola", "alolan" };


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
                if (!Directory.Exists(config.PersisentStorageLocation))
                    Directory.CreateDirectory(config.PersisentStorageLocation);
            }
            catch (Exception e)
            {
                $"Error when creating persistant storage location at {config.PersisentStorageLocation}: {e.Message}".Log();
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

		public Gym GetGymNameFromSearchString(string channel, string searchString)
		{
			foreach (var location in Locations)
			{
				if (!location.Channels.Contains(channel))
					continue;

				// Exact matches on name
				foreach (var gym in location.Gyms)
					if (string.Equals(gym.Name, searchString, StringComparison.InvariantCultureIgnoreCase))
						return gym;

				// Exact match on alias
				foreach (var gym in location.Gyms)
					foreach (var alias in gym.Aliases)
						if (string.Equals(alias, searchString, StringComparison.InvariantCultureIgnoreCase))
							return gym;

				int partialMatches;
				Gym partialMatchedGym;

				// StartsWith match on name
				partialMatches = 0;
				partialMatchedGym = null;
				foreach (var gym in location.Gyms)
				{
					if (gym.Name.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase))
					{
						partialMatchedGym = gym;
						partialMatches++;
					}
				}
				if (partialMatches == 1) return partialMatchedGym;

				// StartsWith match on alias
				partialMatches = 0;
				partialMatchedGym = null;
				foreach (var gym in location.Gyms)
				{
					foreach (var alias in gym.Aliases)
					{
						if (alias.StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase))
						{
							partialMatchedGym = gym;
							partialMatches++;
						}
					}
				}
				if (partialMatches == 1) return partialMatchedGym;

				// Partial match on name
				partialMatches = 0;
				partialMatchedGym = null;
				foreach (var gym in location.Gyms)
				{
					if (gym.Name.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0)
					{
						partialMatchedGym = gym;
						partialMatches++;
					}
				}
				if (partialMatches == 1) return partialMatchedGym;

				// Partial match on alias
				partialMatches = 0;
				partialMatchedGym = null;
				foreach (var gym in location.Gyms)
				{
					foreach (var alias in gym.Aliases)
					{
						if (alias.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0)
						{
							partialMatchedGym = gym;
							partialMatches++;
						}
					}
				}
				if (partialMatches == 1) return partialMatchedGym;

			}
			return null;
		}
	}

	public class Location
	{
		[JsonProperty("channels")]
		public List<string> Channels { get; set; }
		[JsonProperty("gyms")]
		public List<Gym> Gyms { get; set; }
	}

	public class Gym
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("latitude")]
		public double Latitude { get; set; }
		[JsonProperty("longitude")]
		public double Longitude { get; set; }
		[JsonProperty("aliases")]
		public string[] Aliases { get; set; }

	}

}
