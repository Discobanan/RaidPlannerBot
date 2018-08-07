using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Discord.WebSocket;
using Discord.Rest;
using System.Text.RegularExpressions;

namespace RaidPlannerBot
{
    public class PlanCollection
    {
        private readonly Dictionary<Tuple<ulong, ulong, ulong>, Plan> list = new Dictionary<Tuple<ulong, ulong, ulong>, Plan>();

        public void Add(ulong guildId, ulong channelId, ulong messageId, Plan plan)
        {
            list.Add(new Tuple<ulong, ulong, ulong>(guildId, channelId, messageId), plan);

            SavePlan(guildId, channelId, messageId, plan);
        }

        public void Update(ulong guildId, ulong channelId, ulong messageId, Plan plan)
        {
            SavePlan(guildId, channelId, messageId, plan);
        }

        public void Remove(ulong guildId, ulong channelId, ulong messageId)
        {
            list.Remove(new Tuple<ulong, ulong, ulong>(guildId, channelId, messageId));

            RemoveSavedPlan(guildId, channelId, messageId);
        }

        public bool Contains(ulong guildId, ulong channelId, ulong messageId)
        {
            return list.ContainsKey(new Tuple<ulong, ulong, ulong>(guildId, channelId, messageId));
        }

        public Plan Get(ulong guildId, ulong channelId, ulong messageId)
        {
            return list[new Tuple<ulong, ulong, ulong>(guildId, channelId, messageId)];
        }

        public Plan Edit(string channel, string message)
        {
			var oldFormat = new Regex(@"^!edit +(?<id>[0-9]+) +(?<value>\S+)$", RegexOptions.IgnoreCase);
			var newFormat = new Regex(@"^!edit (?<command>time|boss|location) +(?<id>[0-9]+) +(?<value>.+)$", RegexOptions.IgnoreCase);

			var oldFormatMatch = oldFormat.Match(message);
			var newFormatMatch = newFormat.Match(message);

			if (!oldFormatMatch.Success && !newFormatMatch.Success)
				return null;

			int id;
			string command;
			string value;

			if (oldFormatMatch.Success)
			{
				int.TryParse(oldFormatMatch.Groups["id"].Value, out id);
				command = "time";
				value = oldFormatMatch.Groups["value"].Value;
			}
			else
			{
				int.TryParse(newFormatMatch.Groups["id"].Value, out id);
				command = newFormatMatch.Groups["command"].Value.ToLower();
				value = newFormatMatch.Groups["value"].Value;
			}

			var plan = list.Where(x => x.Value.Id == id).Select(x => x.Value).FirstOrDefault();

			if (plan == null)
				return null;

			if (command == "time") plan.Time = value;
			else if (command == "boss") plan.Pokemon = value;
			else if (command == "location")
			{
				var gym = AppConfig.Shared.GetGymNameFromSearchString(channel, value);
				plan.Latitude = gym?.Latitude ?? 0;
				plan.Longitude = gym?.Longitude ?? 0;
				plan.Location = gym?.Name ?? value;
			}

			return plan;
        }

        public void DeleteExpired()
        {
            var allKeys = list.Keys.ToList();
            foreach (var tuple in allKeys)
            {
                var guildId = tuple.Item1;
                var channelId = tuple.Item2;
                var messageId = tuple.Item3;
                var plan = list[tuple];

                var planAge = DateTime.Now.Subtract(plan.CreatedDate);
                var isExpiredRaid = !plan.IsExRaid && planAge.TotalMinutes > AppConfig.Shared.PlanExpirationMinutes;
                var isExpiredExRaid = plan.IsExRaid && planAge.TotalDays > AppConfig.Shared.ExPlanExpirationDays;

                if (isExpiredRaid || isExpiredExRaid)
                {
                    $"Removing expired plan for {plan.Pokemon} at {plan.Location}, {plan.Time}!".Log();

                    Remove(guildId, channelId, messageId);

                    try { plan.Message.DeleteAsync(); }
                    catch(Exception) { } // Don't care if it fails, it have probably been deleted manually
                }
            }
        }

        public void LoadPlansForGuild(SocketGuild guild)
        {
            var path = Path.Combine(AppConfig.Shared.PersisentStorageLocation, guild.Id.ToString());

            if (!Directory.Exists(path))
                return;

            $"Loading plans for guild {guild.Name}...".Log();

            var guildDir = new DirectoryInfo(path);
            var count = 0;

            foreach (var channelDir in guildDir.GetDirectories())
            {
                var channelId = ulong.Parse(channelDir.Name);
                var socketChannel = guild.GetChannel(channelId) as ISocketMessageChannel;

                foreach (var messageFile in channelDir.GetFiles())
                {
                    var messageId = ulong.Parse(messageFile.Name);
                    var key = new Tuple<ulong, ulong, ulong>(guild.Id, channelId, messageId);

                    
                    
                    var json = File.ReadAllText(messageFile.FullName);
                    var plan = JsonConvert.DeserializeObject<Plan>(json);

                    try
                    {
                        RestUserMessage message = (RestUserMessage)socketChannel.GetMessageAsync(messageId).Result;
                        plan.Message = message;

                        if (!list.ContainsKey(key)) list.Add(key, plan);
                        else list[key] = plan;

                        // TODO: Read reactions and repost the plan to discord, since reactions might have changed

                        count++;
                    }
                    catch (Exception e)
                    {
                        $"Could not read messageId {messageId} from channel {channelId} on guild {guild.Id}: {e.Message}".Log();
                    }

                }
                
            }

            $"Loaded {count} plan(s)".Log();
        }

        private void SavePlan(ulong guildId, ulong channelId, ulong messageId, Plan plan)
        {
            var guildDir = Path.Combine(AppConfig.Shared.PersisentStorageLocation, guildId.ToString());
            if (!Directory.Exists(guildDir)) Directory.CreateDirectory(guildDir);

            var channelDir = Path.Combine(AppConfig.Shared.PersisentStorageLocation, guildDir, channelId.ToString());
            if (!Directory.Exists(channelDir)) Directory.CreateDirectory(channelDir);

            var planFile = Path.Combine(AppConfig.Shared.PersisentStorageLocation, guildDir, channelId.ToString(), messageId.ToString());
            if (File.Exists(planFile)) File.Delete(planFile);
            var json = JsonConvert.SerializeObject(plan);
            File.WriteAllText(planFile, json);

            $"Wrote plan file {planFile}".Log(true);
        }

        private void RemoveSavedPlan(ulong guildId, ulong channelId, ulong messageId)
        {
            var planFile = Path.Combine(AppConfig.Shared.PersisentStorageLocation, guildId.ToString(), channelId.ToString(), messageId.ToString());
            if (File.Exists(planFile)) File.Delete(planFile);

            $"Deleted plan file {planFile}".Log(true);
        }

    }
}
