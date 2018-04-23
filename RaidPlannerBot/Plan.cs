using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;

namespace RaidPlannerBot
{
    [Serializable]
    public class Plan
    {
        public DateTime CreatedDate { get; set; }

        public string Pokemon { get; set; }
        public string Time { get; set; }
        public string Day { get; set; }
        public string Location { get; set; }
        public string Author { get; set; }
        public string Discriminator { get; set; }
        public List<string> Mystic { get; set; }
        public List<string> Valor { get; set; }
        public List<string> Instinct { get; set; }
        public int Unknowns { get; set; }
        public bool IsExRaid { get; set; }

        [JsonIgnore]
        public RestUserMessage Message { get; set; }

        [JsonIgnore]
        public int Total => Mystic.Count() + Valor.Count() + Instinct.Count() + Unknowns;

        [JsonIgnore]
        public int Id => Math.Abs(CreatedDate.GetHashCode()) % 10000; // TODO: Incremental id

        public Plan()
        {
            this.CreatedDate = DateTime.Now;
            this.Mystic = new List<string>();
            this.Valor = new List<string>();
            this.Instinct = new List<string>();
        }

        public static Plan Create(string message, string author, string discriminator, bool isExRaid)
        {
            var messageParts = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if ((!isExRaid && messageParts.Length < 4) || (isExRaid && messageParts.Length < 5))
                return null;

            return new Plan()
            {
                Pokemon = messageParts[1],
                Time = messageParts[2],
                Location = String.Join(" ", messageParts.Skip(isExRaid ? 4 : 3).ToArray()),
                Author = author,
                Discriminator = discriminator,
                IsExRaid = isExRaid,
                Day = isExRaid ? messageParts[3] : null,
            };
        }

        public Embed AsDiscordEmbed()
        {
            var title = IsExRaid ? $"EX-RAID: {this.Pokemon}" : $"Boss: {this.Pokemon}";

            var description = $"Time: {this.Time}\nLocation: {this.Location}";
            if (IsExRaid) description += $"\nDay: {this.Day}";

            var footer = $"Total: {this.Total} | Id: {this.Id} | Created by: {this.Author}#{this.Discriminator}";

            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithTitle(title);
            embedBuilder.WithDescription(description);
            embedBuilder.WithFooter(footer);

            var pokemonName = this.Pokemon.ToLower();
            if (Pokemons.Name.ContainsKey(pokemonName))
            {
                var thumbnailUrl = AppConfig.Shared.ThumbnailUrl.Replace("{pokemonId}", Pokemons.Name[pokemonName].ToString());
                if (Uri.IsWellFormedUriString(thumbnailUrl, UriKind.Absolute))
                {
                    embedBuilder.WithThumbnailUrl(thumbnailUrl);
                }
            }

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = $"Mystic ({this.Mystic.Count()})",
                Value = $"[{String.Join(", ", this.Mystic)}]",
                IsInline = true
            });

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = $"Valor ({this.Valor.Count()})",
                Value = $"[{String.Join(", ", this.Valor)}]",
                IsInline = true
            });

            embedBuilder.AddField(new EmbedFieldBuilder()
            {
                Name = $"Instinct ({this.Instinct.Count()})",
                Value = $"[{String.Join(", ", this.Instinct)}]",
                IsInline = true
            });

            return embedBuilder;
        }
    }
}
