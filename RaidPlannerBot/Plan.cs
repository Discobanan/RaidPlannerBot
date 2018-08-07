using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Rest;
using Newtonsoft.Json;
using System.Globalization;
using RaidPlannerBot.Data;

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
		public double Latitude { get; set; }
		public double Longitude { get; set; }
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

        public static Plan Create(string channel, string message, string author, string discriminator, bool isExRaid)
        {
            var messageParts = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if ((!isExRaid && messageParts.Length < 4) || (isExRaid && messageParts.Length < 5))
                return null;

			var location = String.Join(" ", messageParts.Skip(isExRaid ? 4 : 3).ToArray());
			var gym = AppConfig.Shared.GetGymNameFromSearchString(channel, location);

			return new Plan()
            {
                Pokemon = messageParts[1],
                Time = messageParts[2],
                Location = gym?.Name ?? location,
                Author = author,
                Discriminator = discriminator,
                IsExRaid = isExRaid,
                Day = isExRaid ? messageParts[3] : null,
				Latitude = gym?.Latitude ?? 0,
				Longitude = gym?.Longitude ?? 0,
			};
        }

        public Embed AsDiscordEmbed()
        {
			NumberFormatInfo nfi = new NumberFormatInfo();
			nfi.NumberDecimalSeparator = ".";
			var latString = Latitude.ToString(nfi);
			var lngString = Longitude.ToString(nfi);
			var mapsLink = $"https://www.google.com/maps/?q={latString},{lngString}";

			var cpAt20 = GameMaster.GetCpAtLevel(this.Pokemon, 20);
			var cpAt25 = GameMaster.GetCpAtLevel(this.Pokemon, 25);

			var title = IsExRaid ? $"EX-RAID: {this.Pokemon.Capitalize()}" : $"Boss: {this.Pokemon.Capitalize()}";

			var description = string.Empty;
			description += $"Time: {this.Time}";
			description += Latitude != 0 && Longitude != 0 ? $"\nLocation: [{this.Location}]({mapsLink})" : $"\nLocation: {this.Location}";
			if (IsExRaid) description += $"\nDay: {this.Day}";
			if (cpAt20 > 0 && cpAt25 > 0) description += $"\nCP at max IV: {cpAt20} / {cpAt25}";

			var footer = $"Total: {this.Total} | Id: {this.Id} | Created by: {this.Author}#{this.Discriminator}";

            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithTitle(title);
            embedBuilder.WithDescription(description);
			embedBuilder.WithFooter(footer);

			var pokemonId = Pokemons.GetIdFromName(this.Pokemon);
            if (!string.IsNullOrEmpty(pokemonId))
            {
				var thumbnailUrl = AppConfig.Shared.ThumbnailUrl.Replace("{pokemonId}", pokemonId);
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
