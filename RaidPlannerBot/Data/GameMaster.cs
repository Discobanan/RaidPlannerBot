using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RaidPlannerBot.Data
{
	public static class GameMaster
	{
		private static GameMasterModel data;
		public static GameMasterModel Data
		{
			get
			{
				if (data == null)
				{

					var dirname = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Data");
					var filename = Path.Combine(dirname, "GAME_MASTER.json");
					var json = File.ReadAllText(filename);

					data = JsonConvert.DeserializeObject<GameMasterModel>(json);
				}

				return data;
			}
		}

		public static Dictionary<double, double> CpMultipliers = new Dictionary<double, double>
		{
			{ 1, 0.094 }, { 1.5, 0.135137432 }, { 2, 0.16639787 }, { 2.5, 0.192650919 }, { 3, 0.21573247 }, { 3.5, 0.236572661 }, { 4, 0.25572005 }, { 4.5, 0.273530381 }, { 5, 0.29024988 }, { 5.5, 0.306057377 }, { 6, 0.3210876 }, { 6.5, 0.335445036 }, { 7, 0.34921268 }, { 7.5, 0.362457751 }, { 8, 0.37523559 }, { 8.5, 0.387592406 }, { 9, 0.39956728 }, { 9.5, 0.411193551 }, { 10, 0.42250001 }, { 10.5, 0.432926419 }, { 11, 0.44310755 }, { 11.5, 0.4530599578 }, { 12, 0.46279839 }, { 12.5, 0.472336083 }, { 13, 0.48168495 }, { 13.5, 0.4908558 }, { 14, 0.49985844 }, { 14.5, 0.508701765 }, { 15, 0.51739395 }, { 15.5, 0.525942511 }, { 16, 0.53435433 }, { 16.5, 0.542635767 }, { 17, 0.55079269 }, { 17.5, 0.558830576 }, { 18, 0.56675452 }, { 18.5, 0.574569153 }, { 19, 0.58227891 }, { 19.5, 0.589887917 }, { 20, 0.59740001 }, { 20.5, 0.604818814 }, { 21, 0.61215729 }, { 21.5, 0.619399365 }, { 22, 0.62656713 }, { 22.5, 0.633644533 }, { 23, 0.64065295 }, { 23.5, 0.647576426 }, { 24, 0.65443563 }, { 24.5, 0.661214806 }, { 25, 0.667934 }, { 25.5, 0.674577537 }, { 26, 0.68116492 }, { 26.5, 0.687680648 }, { 27, 0.69414365 }, { 27.5, 0.700538673 }, { 28, 0.70688421 }, { 28.5, 0.713164996 }, { 29, 0.71939909 }, { 29.5, 0.725571552 }, { 30, 0.7317 }, { 30.5, 0.734741009 }, { 31, 0.73776948 }, { 31.5, 0.740785574 }, { 32, 0.74378943 }, { 32.5, 0.746781211 }, { 33, 0.74976104 }, { 33.5, 0.752729087 }, { 34, 0.75568551 }, { 34.5, 0.758630378 }, { 35, 0.76156384 }, { 35.5, 0.764486065 }, { 36, 0.76739717 }, { 36.5, 0.770297266 }, { 37, 0.7731865 }, { 37.5, 0.776064962 }, { 38, 0.77893275 }, { 38.5, 0.781790055 }, { 39, 0.78463697 }, { 39.5, 0.787473578 }, { 40, 0.79030001 }
		};

		public static int CalculateLevel(double cpMultiplier)
		{
			double level;
			if (cpMultiplier < 0.734)
				level = 58.35178527 * cpMultiplier * cpMultiplier - 2.838007664 * cpMultiplier + 0.8539209906;
			else
				level = 171.0112688 * cpMultiplier - 95.20425243;

			return (int)((Math.Round(level) * 2) / 2.0);
		}

		public static int GetCpAtLevel(string pokemonName, int level = 40, int attack = 15, int defense = 15, int stamina = 15)
		{
			if (attack > 0 || defense > 0 || stamina > 0)
			{
				var nameParts = pokemonName.Split('-');

				string formName = null;
				if (AppConfig.AlolanPrefixes.Contains(nameParts[0]))
				{
					pokemonName = String.Join("-", nameParts.Skip(1).ToArray()).ToUpper();
					formName = pokemonName + "_ALOLA";
				}

				var baseStats = GameMaster
					.Data
					.ItemTemplates
					.Where(x => x.PokemonSettings?.PokemonId == pokemonName.ToUpper() && x.PokemonSettings?.Form == formName)
					.Select(x => x.PokemonSettings.Stats)
					.FirstOrDefault();

				if (baseStats != null)
				{
					var cpMultiplier = GameMaster.CpMultipliers[level];

					attack += baseStats.BaseAttack;
					defense += baseStats.BaseDefense;
					stamina += baseStats.BaseStamina;

					return (int)Math.Floor((attack * Math.Pow(defense, 0.5) * Math.Pow(stamina, 0.5) * Math.Pow(cpMultiplier, 2)) / 10);
				}
			}

			return 0;
		}

		public static string GetEvolvedPokemonName(string pokemonName)
		{
			var familyId = GameMaster
				.Data
				.ItemTemplates
				.Where(x => x.PokemonSettings?.PokemonId == pokemonName.ToUpper())
				.Select(x => x.PokemonSettings.FamilyId)
				.FirstOrDefault();

			if (familyId == null)
			{
				familyId = GameMaster
					.Data
					.ItemTemplates
					.Where(x => x.PokemonSettings?.PokemonId.StartsWith(pokemonName.ToUpper().Substring(0, pokemonName.Length - 1)) ?? false)
					.Select(x => x.PokemonSettings.FamilyId)
					.FirstOrDefault();
			}

			var maxEvolvedPokemonSettings = GameMaster
				.Data
				.ItemTemplates
				.Where(x => x.PokemonSettings?.FamilyId == familyId && x.PokemonSettings?.EvolutionBranch == null)
				.Select(x => x.PokemonSettings)
				.OrderByDescending(x => (x?.Stats?.BaseAttack ?? 0) + (x?.Stats?.BaseDefense ?? 0) + (x?.Stats?.BaseStamina ?? 0))
				.FirstOrDefault();

			var maxEvolvedPokemon = maxEvolvedPokemonSettings?.PokemonId;

			return maxEvolvedPokemon != null ? maxEvolvedPokemon.Capitalize() : null;
		}

		private static Dictionary<int, Tuple<int, int>> generations = new Dictionary<int, Tuple<int, int>>()
		{
			{ 1, new Tuple<int,int>(1,151) },
			{ 2, new Tuple<int,int>(152,251) },
			{ 3, new Tuple<int,int>(252,386) },
			{ 4, new Tuple<int,int>(387,493) },
			{ 5, new Tuple<int,int>(494,649) },
			{ 6, new Tuple<int,int>(650,721) },
			{ 7, new Tuple<int,int>(722,802) }
		};

		public static int PokemonIdToGeneration(int pokemonId)
		{
			foreach (var gen in generations.Keys)
				if (pokemonId >= generations[gen].Item1 && pokemonId <= generations[gen].Item2)
					return gen;

			return 0;
		}

		public static int MinPokemonIdForGeneration(int generation)
		{
			if (generations.ContainsKey(generation))
				return generations[generation].Item1;

			return 0;
		}
		public static int MaxPokemonIdForGeneration(int generation)
		{
			if (generations.ContainsKey(generation))
				return generations[generation].Item2;

			return 0;
		}

	}
}
