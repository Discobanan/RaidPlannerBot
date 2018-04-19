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
        private readonly string persistantStorageLocation;
        public readonly Dictionary<Tuple<ulong, ulong>, Plan> list;

        public PlanCollection(string persistantStorageLocation, DiscordSocketClient discordClient)
        {
            this.persistantStorageLocation = persistantStorageLocation;

            list = new Dictionary<Tuple<ulong, ulong>, Plan>();

            LoadPlans(discordClient);
        }

        public void Add(ulong channelId, ulong messageId, Plan plan)
        {
            list.Add(new Tuple<ulong, ulong>(channelId, messageId), plan);

            SavePlan(channelId, messageId, plan);
        }

        public void Remove(ulong channelId, ulong messageId)
        {
            list.Remove(new Tuple<ulong, ulong>(channelId, messageId));

            RemoveSavedPlan(channelId, messageId);
        }

        public bool Contains(ulong channelId, ulong messageId)
        {
            return list.ContainsKey(new Tuple<ulong, ulong>(channelId, messageId));
        }

        public Plan Get(ulong channelId, ulong messageId)
        {
            return list[new Tuple<ulong, ulong>(channelId, messageId)];
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

        private void LoadPlans(DiscordSocketClient discordClient)
        {
            foreach (var channelDir in new DirectoryInfo(persistantStorageLocation).GetDirectories())
            {
                if (ulong.TryParse(channelDir.Name, out ulong channelId))
                {
                    ISocketMessageChannel socketChannel = (ISocketMessageChannel)discordClient.GetChannel(channelId);

                    foreach (var messageFile in channelDir.GetFiles())
                    {
                        if (ulong.TryParse(messageFile.Name, out ulong messageId))
                        {
                            var json = File.ReadAllText(messageFile.FullName);
                            var plan = JsonConvert.DeserializeObject<Plan>(json);
                            var message = (RestUserMessage)socketChannel.GetMessageAsync(messageId).Result;
                            // TODO: Repost the plan to discord, since reactions might have changed

                            if (message != null)
                            {
                                plan.Message = message;

                                list.Add(new Tuple<ulong, ulong>(channelId, messageId), plan);
                            }
                        }
                    }
                }
            }
        }

        private void SavePlan(ulong channelId, ulong messageId, Plan plan)
        {
            var channelDir = Path.Combine(persistantStorageLocation, channelId.ToString());
            if (!Directory.Exists(channelDir)) Directory.CreateDirectory(channelDir);

            var planFile = Path.Combine(persistantStorageLocation, channelId.ToString(), messageId.ToString());
            if (File.Exists(planFile)) File.Delete(planFile);
            var json = JsonConvert.SerializeObject(plan);
            File.WriteAllText(planFile, json);
        }

        private void RemoveSavedPlan(ulong channelId, ulong messageId)
        {
            var PlanFile = Path.Combine(persistantStorageLocation, channelId.ToString(), messageId.ToString());
            if (File.Exists(PlanFile)) File.Delete(PlanFile);
        }

    }
}
