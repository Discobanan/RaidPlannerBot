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
        public string Location { get; set; }
        public string Author { get; set; }
        public string Discriminator { get; set; }
        public List<string> Mystic { get; set; }
        public List<string> Valor { get; set; }
        public List<string> Instinct { get; set; }
        public int Unknowns { get; set; }

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

        public static Plan Create(string message, string author, string discriminator)
        {
            var messageParts = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (messageParts.Length < 4)
                return null;

            return new Plan()
            {
                Pokemon = messageParts[1],
                Time = messageParts[2],
                Location = String.Join(" ", messageParts.Skip(3).ToArray()),
                Author = author,
                Discriminator = discriminator,
            };
        }

        public Embed AsDiscordEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithTitle(this.Pokemon);
            embedBuilder.WithDescription($"Time: {this.Time}\nLocation: {this.Location}");
            embedBuilder.WithFooter($"Id: {this.Id} | Created by: {this.Author}#{this.Discriminator} | Total: {this.Total}");

            var pokemonName = this.Pokemon.ToLower();
            if (Pokemons.Name.ContainsKey(pokemonName))
                embedBuilder.WithThumbnailUrl($"https://www.teamrocket.nu/static/pokemons/100x100/{Pokemons.Name[pokemonName]}.png");

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

            return embedBuilder; // Return null if message doesn't contain all it must in order to build a proper embed-message
        }
    }
}
