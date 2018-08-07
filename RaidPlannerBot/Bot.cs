using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RaidPlannerBot
{
    public class Bot
    {
        private const int MS_RATE_LIMIT_DELAY = 1500;
        private const int MINUTES_BETWEEN_EXPIRE_CHECKS = 5;
        private const int MINUTES_BETWEEN_DISCONNECTION_CHECKS = 5;

        private readonly DiscordSocketClient discordClient;
        private ulong discordBotUserId;
        private DateTime? disconnectedDate = null;

        private readonly List<string> factionEmojis = new List<string>() { "mystic", "valor", "instinct" };
        private readonly List<string> numberEmojis = new List<string>() { "\u0031\u20E3", "\u0032\u20E3", "\u0033\u20E3", "\u0034\u20E3" };
        private readonly string deleteEmoji = "\u274C";

        private readonly Dictionary<ulong, Dictionary<string, Emote>> guildEmojis = new Dictionary<ulong, Dictionary<string, Emote>>();

        private PlanCollection plans = new PlanCollection();

        public Bot()
        {
            discordClient = new DiscordSocketClient();

            discordClient.Log += BotClient_Log;
            discordClient.Connected += DiscordClient_Connected;
            discordClient.GuildAvailable += BotClient_GuildAvailable;
            discordClient.MessageReceived += BotClient_MessageReceived;
            discordClient.ReactionAdded += DiscordClient_ReactionAdded;
            discordClient.ReactionRemoved += DiscordClient_ReactionRemoved;
            discordClient.Disconnected += DiscordClient_Disconnected;

            // Check for expired plans every X minute, and remove expired plans
            Task.Run(() =>
            {
                while (true)
                {
                    if (plans == null)
                        continue;

                    plans.DeleteExpired();

                    Thread.Sleep(1000 * 60 * MINUTES_BETWEEN_EXPIRE_CHECKS);
                }
            });

            // Check for 
            Task.Run(() =>
            {
                while (true)
                {
                    "Checking if disconnected...".Log(true);

                    if (this.disconnectedDate.HasValue)
                    {
                        var minutesSinceDisconnect = DateTime.Now.Subtract(this.disconnectedDate.Value).TotalMinutes;

                        $"Disconnected {minutesSinceDisconnect} minutes ago!".Log();

                        if (minutesSinceDisconnect > MINUTES_BETWEEN_DISCONNECTION_CHECKS && !string.IsNullOrWhiteSpace(AppConfig.Shared.ExecOnDisconnectFilename))
                        {
                            Process.Start(AppConfig.Shared.ExecOnDisconnectFilename, AppConfig.Shared.ExecOnDisconnectArguments);
                        }
                    }
                    else
                    {
                        "Not disconnected!".Log(true);
                    }

                    Thread.Sleep(1000 * 60 * MINUTES_BETWEEN_DISCONNECTION_CHECKS);
                }
            });
        }

        public void Start()
        {
            discordClient.LoginAsync(TokenType.Bot, AppConfig.Shared.DiscordBotToken).Wait();
            discordClient.StartAsync().Wait();
            discordClient.SetGameAsync(AppConfig.Shared.DiscordPlaying);

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
            plans.LoadPlansForGuild(guild);

            if (!guildEmojis.ContainsKey(guild.Id))
                guildEmojis.Add(guild.Id, new Dictionary<string, Emote>());

            foreach (var emote in guild.Emotes.Where(e => factionEmojis.Contains(e.Name)))
                if (!guildEmojis[guild.Id].ContainsKey(emote.Name))
                    guildEmojis[guild.Id].Add(emote.Name, emote);

            if (guildEmojis[guild.Id].Count() != 3)
                $"Guild {guild.Name} are missing some or all faction emojis!".Log(); // TODO: Create them if they are missing (API's not available in current version of Discord.Net)

            return Task.CompletedTask;
        }

        private Task BotClient_MessageReceived(SocketMessage message)
        {
            if (Regex.IsMatch(message.Content, "^!(?:raid|r|exraid|xr) ", RegexOptions.IgnoreCase))
            {
                var isExRaid = Regex.IsMatch(message.Content, "^!(?:exraid|xr) ");
                var plan = Plan.Create(message.Channel.Name, message.Content, message.Author.Username, message.Author.Discriminator, isExRaid);

                if (plan == null)
                {
                    message.DeleteAsync();
                    return Task.CompletedTask;
                }

                $"Creating plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

                var socketChannel = message.Channel as SocketGuildChannel;

                var replyTask = Task.Run(async () =>
                {
                    try
                    {
                        await message.DeleteAsync();
                        await Task.Delay(MS_RATE_LIMIT_DELAY);

                        RestUserMessage reply = await message.Channel.SendMessageAsync(string.Empty, false, plan.AsDiscordEmbed());
                        plan.Message = reply;
                        plans.Add(socketChannel.Guild.Id, socketChannel.Id, reply.Id, plan);
                        await Task.Delay(MS_RATE_LIMIT_DELAY);

                        foreach (var factionEmoji in factionEmojis)
                        {
                            await reply.AddReactionAsync(guildEmojis[socketChannel.Guild.Id][factionEmoji]);
                            await Task.Delay(MS_RATE_LIMIT_DELAY);
                        }

                        foreach (var numberEmoji in numberEmojis)
                        {
                            await reply.AddReactionAsync(new Emoji(numberEmoji));
                            await Task.Delay(MS_RATE_LIMIT_DELAY);
                        }

                        await reply.AddReactionAsync(new Emoji(deleteEmoji));
                    }
                    catch(Exception e)
                    {
                        $"Exception when replying to message: {e.Message}".Log();
                    }
                });
            }

            if (Regex.IsMatch(message.Content, "^!edit ", RegexOptions.IgnoreCase))
            {
                var plan = plans.Edit(message.Channel.Name, message.Content);

                if (plan == null)
                {
                    message.DeleteAsync();
                    return Task.CompletedTask;
                }

                $"Editing plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

                var editTask = Task.Run(async () =>
                {
                    await message.DeleteAsync();
                    await Task.Delay(MS_RATE_LIMIT_DELAY);
                    await plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
                });
            }

            return Task.CompletedTask;
        }

        private Task DiscordClient_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var socketChannel = channel as SocketGuildChannel;
            var socketUser = reaction.User.Value as SocketGuildUser;

            if (reaction.UserId == discordBotUserId || !plans.Contains(socketChannel.Guild.Id, channel.Id, message.Id))
                return Task.CompletedTask;

            var plan = plans.Get(socketChannel.Guild.Id, channel.Id, message.Id);
            
            var nickname = string.IsNullOrWhiteSpace(socketUser.Nickname) ? socketUser.Username : socketUser.Nickname;

            if (reaction.Emote.Name == "mystic" && !plan.Mystic.Contains(nickname))
                plan.Mystic.Add(nickname);
            else if (reaction.Emote.Name == "valor" && !plan.Valor.Contains(nickname))
                plan.Valor.Add(nickname);
            else if (reaction.Emote.Name == "instinct" && !plan.Instinct.Contains(nickname))
                plan.Instinct.Add(nickname);

            for(int i = 0; i < numberEmojis.Count; i++)
                if (reaction.Emote.Name == numberEmojis[i])
                    plan.Unknowns = plan.Unknowns + (i + 1);

            // TODO: Allow some role to also delete plans, not only the creator
            if (reaction.Emote.Name == deleteEmoji && socketUser.Username == plan.Author && reaction.User.Value.Discriminator == plan.Discriminator)
            {
                plan.Message.DeleteAsync();
                plans.Remove(socketChannel.Guild.Id, channel.Id, message.Id);

                $"{nickname} removed plan for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);
            }
            else
            {
                plans.Update(socketChannel.Guild.Id, channel.Id, message.Id, plan);
                $"{nickname} added a reaction for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);
            }

            return plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
        }

        private Task DiscordClient_ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var socketChannel = channel as SocketGuildChannel;
            var socketUser = reaction.User.Value as SocketGuildUser;

            if (reaction.UserId == discordBotUserId || !plans.Contains(socketChannel.Guild.Id, channel.Id, message.Id))
                return Task.CompletedTask;

            var plan = plans.Get(socketChannel.Guild.Id, channel.Id, message.Id);
            var nickname = string.IsNullOrWhiteSpace(socketUser.Nickname) ? socketUser.Username : socketUser.Nickname;

            if (reaction.Emote.Name == "mystic" && plan.Mystic.Contains(nickname))
                plan.Mystic.Remove(nickname);
            else if (reaction.Emote.Name == "valor" && plan.Valor.Contains(nickname))
                plan.Valor.Remove(nickname);
            else if (reaction.Emote.Name == "instinct" && plan.Instinct.Contains(nickname))
                plan.Instinct.Remove(nickname);

            for (int i = 0; i < numberEmojis.Count; i++)
                if (reaction.Emote.Name == numberEmojis[i])
                    plan.Unknowns = plan.Unknowns - (i + 1);

            $"{nickname} removed a reaction for {plan.Pokemon} at {plan.Location}, {plan.Time}".Log(true);

            plans.Update(socketChannel.Guild.Id, channel.Id, message.Id, plan);
            return plan.Message.ModifyAsync(m => m.Embed = plan.AsDiscordEmbed());
        }

        private Task DiscordClient_Connected()
        {
            this.disconnectedDate = null;

            if (discordBotUserId == 0)
                discordBotUserId = discordClient.CurrentUser.Id;

            return Task.CompletedTask;
        }

        private Task DiscordClient_Disconnected(Exception arg)
        {
            this.disconnectedDate = DateTime.Now;

            return Task.CompletedTask;
        }

    }
}
