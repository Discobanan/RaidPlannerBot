using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Discord.WebSocket;
using Discord.Rest;

namespace RaidPlannerBot
{
    class PlanCollection
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

        public Plan Edit(string message)
        {
            var messageParts = message.Split(" ");

            if (messageParts.Length < 3)
                return null;

            if (!int.TryParse(messageParts[1], out int id))
                return null;

            var newTime = messageParts[2];

            var plan = list.Where(x => x.Value.Id == id).Select(x => x.Value).FirstOrDefault();

            if (plan != null)
                plan.Time = newTime;

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
                var planAgeMinutes = DateTime.Now.Subtract(plan.CreatedDate).TotalMinutes;

                if (planAgeMinutes > AppConfig.Shared.PlanExpirationMinutes)
                {
                    $"Removing expired plan for {plan.Pokemon} at {plan.Location}, {plan.Time}!".Log();

                    plan.Message.DeleteAsync();
                    Remove(guildId, channelId, messageId);
                }
                else
                {
                    $"Plan for {plan.Pokemon} at {plan.Location}, {plan.Time} is only {Math.Round(planAgeMinutes)} minutes old, won't be removed".Log(true);
                }
            }
        }

        public void LoadPlansForGuild(SocketGuild guild)
        {
            $"Loading plans for guild {guild.Name}...".Log();

            var count = 0;
            var guildId = guild.Id;
            var guildDir = new DirectoryInfo(Path.Combine(AppConfig.Shared.PlanPersisentStorageLocation, guildId.ToString()));
            foreach (var channelDir in guildDir.GetDirectories())
            {
                var channelId = ulong.Parse(channelDir.Name);

                var socketChannel = guild.GetChannel(channelId) as ISocketMessageChannel;

                foreach (var messageFile in channelDir.GetFiles())
                {
                    if (ulong.TryParse(messageFile.Name, out ulong messageId))
                    {
                        var json = File.ReadAllText(messageFile.FullName);
                        var plan = JsonConvert.DeserializeObject<Plan>(json);

                        try
                        {
                            RestUserMessage message = (RestUserMessage)socketChannel.GetMessageAsync(messageId).Result;
                            plan.Message = message;
                            list.Add(new Tuple<ulong, ulong, ulong>(guildId, channelId, messageId), plan);
                            // TODO: Repost the plan to discord, since reactions might have changed
                            count++;
                        }
                        catch (Exception e)
                        {
                            $"Could not read messageId {messageId} from channel {channelId} on guild {guildId}: {e.Message}".Log();
                        }
                    }
                }
                
            }

            $"Loaded {count} plan(s)".Log();
        }

        private void SavePlan(ulong guildId, ulong channelId, ulong messageId, Plan plan)
        {
            var guildDir = Path.Combine(AppConfig.Shared.PlanPersisentStorageLocation, guildId.ToString());
            if (!Directory.Exists(guildDir)) Directory.CreateDirectory(guildDir);

            var channelDir = Path.Combine(AppConfig.Shared.PlanPersisentStorageLocation, guildDir.ToString(), channelId.ToString());
            if (!Directory.Exists(channelDir)) Directory.CreateDirectory(channelDir);

            var planFile = Path.Combine(AppConfig.Shared.PlanPersisentStorageLocation, guildDir.ToString(), channelId.ToString(), messageId.ToString());
            if (File.Exists(planFile)) File.Delete(planFile);
            var json = JsonConvert.SerializeObject(plan);
            File.WriteAllText(planFile, json);
        }

        private void RemoveSavedPlan(ulong guildId, ulong channelId, ulong messageId)
        {
            var PlanFile = Path.Combine(AppConfig.Shared.PlanPersisentStorageLocation, guildId.ToString(), channelId.ToString(), messageId.ToString());
            if (File.Exists(PlanFile)) File.Delete(PlanFile);
        }

    }
}
