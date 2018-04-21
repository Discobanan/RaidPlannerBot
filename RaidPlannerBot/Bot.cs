using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RaidPlannerBot
{
    public class Bot
    {
        private const int RATE_LIMIT_DELAY = 1500;

        private readonly DiscordSocketClient discordClient;

        private readonly List<string> factionEmojis = new List<string>() { "mystic", "valor", "instinct" };
        private readonly List<string> numberEmojis = new List<string>() { "\u0031\u20E3", "\u0032\u20E3", "\u0033\u20E3", "\u0034\u20E3" };
        private readonly string deleteEmoji = "\u274C";

        private readonly Dictionary<ulong, ulong> guildChannels = new Dictionary<ulong, ulong>();
        private readonly Dictionary<ulong, Dictionary<string, Emote>> guildEmojis = new Dictionary<ulong, Dictionary<string, Emote>>();

        private PlanCollection plans;

        public Bot()
        {
            discordClient = new DiscordSocketClient();

            discordClient.Log += BotClient_Log;
            discordClient.GuildAvailable += BotClient_GuildAvailable;
            discordClient.MessageReceived += BotClient_MessageReceived;
            discordClient.ReactionAdded += DiscordClient_ReactionAdded;
            discordClient.ReactionRemoved += DiscordClient_ReactionRemoved;

            // Check for expired plans every minute, and remove expired plans
            Task.Run(() =>
            {
                while (true)
                {
                    if (plans == null || plans.list == null)
                        continue;

                    var allKeys = plans.list.Keys.ToList();
                    foreach (var tuple in allKeys)
                    {
                        var channelId = tuple.Item1;
                        var messageId = tuple.Item2;
                        var plan = plans.list[tuple];
                        var planAgeMinutes = DateTime.Now.Subtract(plan.CreatedDate).TotalMinutes;

                        if (planAgeMinutes > AppConfig.Shared.PlanExpirationMinutes)
                        {
                            $"Removing expired plan for {plan.Pokemon} at {plan.Location}, {plan.Time}!".Log();

                            plan.Message.DeleteAsync();
                            plans.Remove(channelId, messageId);
                        }
                        else
                        {
                            $"Plan for {plan.Pokemon} at {plan.Location}, {plan.Time} is only {Math.Round(planAgeMinutes)} minutes old, won't be removed".Log(true);
                        }
                    }

                    Thread.Sleep(1000 * 60);
                }
            });
        }

        public void Start()
        {
            discordClient.LoginAsync(TokenType.Bot, AppConfig.Shared.DiscordBotToken).Wait();
            discordClient.StartAsync().Wait();
            discordClient.SetGameAsync("Pokemon Go");

            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                "Press ESC to logout the bot!".Log();
            "ESC pressed, logging out...".Log();

            discordClient.StopAsync().Wait();
        }

        private Task BotClient_Log(LogMessage arg)
        {
            arg.Message.Log();

            return Task.CompletedTask;
        }

        private Task BotClient_GuildAvailable(SocketGuild guild)
        {
            plans = new PlanCollection(discordClient);

            foreach (var channel in guild.Channels)
                if (!guildChannels.ContainsKey(channel.Id))
                    guildChannels.Add(channel.Id, guild.Id);

            if (!guildEmojis.ContainsKey(guild.Id))
                guildEmojis.Add(guild.Id, new Dictionary<string, Emote>());

            foreach (var emote in guild.Emotes.Where(e => factionEmojis.Contains(e.Name)))
                if (!guildEmojis[guild.Id].ContainsKey(emote.Name))
                    guildEmojis[guild.Id].Add(emote.Name, emote);

            if (guildEmojis[guild.Id].Count() != 3)
                $"Faction emojis are missing!".Log(); // TODO: Create them if they are missing

            return Task.CompletedTask;
        }

        private Task BotClient_MessageReceived(SocketMessage message)
        {
            if (Regex.IsMatch(message.Content, "^!(raid|r) "))
            {
                var plan = Plan.Create(message.Content, message.Author.Username, message.Author.Discriminator);

                if (plan == null)
                {
                    message.DeleteAsync();
                    return Task.CompletedTask;
                }

                $"Creating plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

                var guildId = guildChannels[message.Channel.Id];

                var replyTask = Task.Run(async () =>
                {
                    await message.DeleteAsync();
                    await Task.Delay(RATE_LIMIT_DELAY);

                    RestUserMessage reply = await message.Channel.SendMessageAsync(string.Empty, false, plan.AsDiscordEmbed());
                    plan.Message = reply;
                    plans.Add(message.Channel.Id, reply.Id, plan);
                    await Task.Delay(RATE_LIMIT_DELAY);

                    foreach (var factionEmoji in factionEmojis)
                    {
                        await reply.AddReactionAsync(guildEmojis[guildId][factionEmoji]);
                        await Task.Delay(RATE_LIMIT_DELAY);
                    }

                    foreach (var numberEmoji in numberEmojis)
                    {
                        await reply.AddReactionAsync(new Emoji(numberEmoji));
                        await Task.Delay(RATE_LIMIT_DELAY);
                    }

                    await reply.AddReactionAsync(new Emoji(deleteEmoji));
                });
            }

            if (Regex.IsMatch(message.Content, "^!edit "))
            {
                var plan = plans.Edit(message.Content);

                if (plan == null)
                {
                    message.DeleteAsync();
                    return Task.CompletedTask;
                }

                $"Editing plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

                var editTask = Task.Run(async () =>
                {
                    await message.DeleteAsync();
                    await Task.Delay(RATE_LIMIT_DELAY);
                    await plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
                });
            }

            return Task.CompletedTask;
        }

        private Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == discordClient.CurrentUser.Id || !plans.Contains(channel.Id, message.Id))
                return Task.CompletedTask;

            var plan = plans.Get(channel.Id, message.Id);
            var username = reaction.User.Value.Username;

            if (reaction.Emote.Name == "mystic" && !plan.Mystic.Contains(username))
                plan.Mystic.Add(username);
            else if (reaction.Emote.Name == "valor" && !plan.Valor.Contains(username))
                plan.Valor.Add(username);
            else if (reaction.Emote.Name == "instinct" && !plan.Instinct.Contains(username))
                plan.Instinct.Add(username);

            for(int i = 0; i < numberEmojis.Count; i++)
                if (reaction.Emote.Name == numberEmojis[i])
                    plan.Unknowns = plan.Unknowns + (i + 1);

            // TODO: Allow some role to also delete plans, not only the creator
            if (reaction.Emote.Name == deleteEmoji && username == plan.Author && reaction.User.Value.Discriminator == plan.Discriminator)
            {
                plan.Message.DeleteAsync();
                plans.Remove(channel.Id, message.Id);

                $"{username} removed plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);
            }
            else
            {
                $"{username} added a reaction for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);
            }

            return plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
        }


        private Task DiscordClient_ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == discordClient.CurrentUser.Id || !plans.Contains(channel.Id, message.Id))
                return Task.CompletedTask;

            var plan = plans.Get(channel.Id, message.Id);
            var username = reaction.User.Value.Username;

            if (reaction.Emote.Name == "mystic" && !plan.Mystic.Contains(username))
                plan.Mystic.Remove(username);
            else if (reaction.Emote.Name == "valor" && !plan.Valor.Contains(username))
                plan.Valor.Remove(username);
            else if (reaction.Emote.Name == "instinct" && !plan.Instinct.Contains(username))
                plan.Instinct.Remove(username);

            for (int i = 0; i < numberEmojis.Count; i++)
                if (reaction.Emote.Name == numberEmojis[i])
                    plan.Unknowns = plan.Unknowns - (i + 1);

            $"{username} removed a reaction for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

            return plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
        }

    }
}
