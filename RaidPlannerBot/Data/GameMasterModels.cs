using System;
using System.Collections.Generic;
using System.Text;

namespace RaidPlannerBot.Data
{
	public class AvatarCustomizationModel
	{
		public bool Enabled { get; set; }
		public string AvatarType { get; set; }
		public IList<string> Slot { get; set; }
		public string BundleName { get; set; }
		public string AssetName { get; set; }
		public string GroupName { get; set; }
		public int SortOrder { get; set; }
		public string UnlockType { get; set; }
		public string IapSku { get; set; }
		public string IconName { get; set; }
		public int? UnlockPlayerLevel { get; set; }
	}

	public class BadgeSettingsModel
	{
		public string BadgeType { get; set; }
		public int BadgeRank { get; set; }
		public IList<int> Targets { get; set; }
	}

	public class BattleSettingsModel
	{
		public double RetargetSeconds { get; set; }
		public double EnemyAttackInterval { get; set; }
		public double AttackServerInterval { get; set; }
		public double RoundDurationSeconds { get; set; }
		public double BonusTimePerAllySeconds { get; set; }
		public int MaximumAttackersPerBattle { get; set; }
		public double SameTypeAttackBonusMultiplier { get; set; }
		public int MaximumEnergy { get; set; }
		public double EnergyDeltaPerHealthLost { get; set; }
		public int DodgeDurationMs { get; set; }
		public int MinimumPlayerLevel { get; set; }
		public int SwapDurationMs { get; set; }
		public double DodgeDamageReductionPercent { get; set; }
	}

	public class EncounterSettingsModel
	{
		public double SpinBonusThreshold { get; set; }
		public double ExcellentThrowThreshold { get; set; }
		public double GreatThrowThreshold { get; set; }
		public double NiceThrowThreshold { get; set; }
		public int MilestoneThreshold { get; set; }
	}

	public class FormModel
	{
		public string Form { get; set; }
		public int AssetBundleValue { get; set; }
	}

	public class FormSettingsModel
	{
		public string Pokemon { get; set; }
		public IList<FormModel> Forms { get; set; }
	}

	public class GymLevelModel
	{
		public IList<int> RequiredExperience { get; set; }
		public IList<int> LeaderSlots { get; set; }
		public IList<int> TrainerSlots { get; set; }
	}

	public class IapSettingsModel
	{
		public IList<int> DailyDefenderBonusPerPokemon { get; set; }
		public int DailyDefenderBonusMaxDefenders { get; set; }
		public IList<string> DailyDefenderBonusCurrency { get; set; }
		public string MinTimeBetweenClaimsMs { get; set; }
		public bool DailyDefenderBonusEnabled { get; set; }
	}

	public class FoodModel
	{
		public IList<string> ItemEffect { get; set; }
		public IList<double> ItemEffectPercent { get; set; }
		public double GrowthPercent { get; set; }
	}

	public class PotionModel
	{
		public int StaAmount { get; set; }
		public double? StaPercent { get; set; }
	}

	public class IncenseModel
	{
		public int IncenseLifetimeSeconds { get; set; }
		public int StandingTimeBetweenEncountersSeconds { get; set; }
		public int MovingTimeBetweenEncounterSeconds { get; set; }
		public int DistanceRequiredForShorterIntervalMeters { get; set; }
	}

	public class EggIncubatorModel
	{
		public string IncubatorType { get; set; }
		public int Uses { get; set; }
		public double DistanceMultiplier { get; set; }
	}

	public class InventoryUpgradeModel
	{
		public int AdditionalStorage { get; set; }
		public string UpgradeType { get; set; }
	}

	public class XpBoostModel
	{
		public double XpMultiplier { get; set; }
		public int BoostDurationMs { get; set; }
	}

	public class ReviveModel
	{
		public double StaPercent { get; set; }
	}

	public class ItemSettingsModel
	{
		public string ItemId { get; set; }
		public string ItemType { get; set; }
		public string Category { get; set; }
		public int DropTrainerLevel { get; set; }
		public FoodModel Food { get; set; }
		public PotionModel Potion { get; set; }
		public IncenseModel Incense { get; set; }
		public EggIncubatorModel EggIncubator { get; set; }
		public InventoryUpgradeModel InventoryUpgrade { get; set; }
		public XpBoostModel XpBoost { get; set; }
		public ReviveModel Revive { get; set; }
	}

	public class PlayerLevelModel
	{
		public IList<int> RankNum { get; set; }
		public IList<int> RequiredExperience { get; set; }
		public IList<double> CpMultiplier { get; set; }
		public int MaxEggPlayerLevel { get; set; }
		public int MaxEncounterPlayerLevel { get; set; }
	}

	public class TypeEffectiveModel
	{
		public IList<double> AttackScalar { get; set; }
		public string AttackType { get; set; }
	}

	public class PokemonUpgradesModel
	{
		public int UpgradesPerLevel { get; set; }
		public int AllowedLevelsAbovePlayer { get; set; }
		public IList<int> CandyCost { get; set; }
		public IList<int> StardustCost { get; set; }
	}

	public class DailyQuestModel
	{
		public int BucketsPerDay { get; set; }
		public int StreakLength { get; set; }
		public double? BonusMultiplier { get; set; }
		public double? StreakBonusMultiplier { get; set; }
	}

	public class QuestSettingsModel
	{
		public string QuestType { get; set; }
		public DailyQuestModel DailyQuest { get; set; }
	}

	public class GenderModel
	{
		public double MalePercent { get; set; }
		public double FemalePercent { get; set; }
		public double? GenderlessPercent { get; set; }
	}

	public class GenderSettingsModel
	{
		public string Pokemon { get; set; }
		public GenderModel Gender { get; set; }
	}

	public class CameraModel
	{
		public double DiskRadiusM { get; set; }
		public double CylinderRadiusM { get; set; }
		public double CylinderHeightM { get; set; }
		public double ShoulderModeScale { get; set; }
		public double? CylinderGroundM { get; set; }
	}

	public class EncounterModel
	{
		public double BaseCaptureRate { get; set; }
		public double BaseFleeRate { get; set; }
		public double CollisionRadiusM { get; set; }
		public double CollisionHeightM { get; set; }
		public double CollisionHeadRadiusM { get; set; }
		public string MovementType { get; set; }
		public double MovementTimerS { get; set; }
		public double JumpTimeS { get; set; }
		public double AttackTimerS { get; set; }
		public double AttackProbability { get; set; }
		public double DodgeProbability { get; set; }
		public double DodgeDurationS { get; set; }
		public double DodgeDistance { get; set; }
		public double CameraDistance { get; set; }
		public double MinPokemonActionFrequencyS { get; set; }
		public double MaxPokemonActionFrequencyS { get; set; }
		public int? BonusCandyCaptureReward { get; set; }
		public int? BonusStardustCaptureReward { get; set; }
	}

	public class StatsModel
	{
		public int BaseStamina { get; set; }
		public int BaseAttack { get; set; }
		public int BaseDefense { get; set; }
	}

	public class EvolutionBranchModel
	{
		public string Evolution { get; set; }
		public int CandyCost { get; set; }
		public string EvolutionItemRequirement { get; set; }
	}

	public class PokemonSettingsModel
	{
		public string PokemonId { get; set; }
		public double ModelScale { get; set; }
		public string Type { get; set; }
		public string Type2 { get; set; }
		public CameraModel Camera { get; set; }
		public EncounterModel Encounter { get; set; }
		public StatsModel Stats { get; set; }
		public IList<string> QuickMoves { get; set; }
		public IList<string> CinematicMoves { get; set; }
		public IList<double> AnimationTime { get; set; }
		public IList<string> EvolutionIds { get; set; }
		public int EvolutionPips { get; set; }
		public double PokedexHeightM { get; set; }
		public double PokedexWeightKg { get; set; }
		public double HeightStdDev { get; set; }
		public double WeightStdDev { get; set; }
		public string FamilyId { get; set; }
		public int CandyToEvolve { get; set; }
		public double KmBuddyDistance { get; set; }
		public double ModelHeight { get; set; }
		public IList<EvolutionBranchModel> EvolutionBranch { get; set; }
		public string ParentPokemonId { get; set; }
		public string BuddySize { get; set; }
		public string Rarity { get; set; }
		public string Form { get; set; }
	}

	public class MoveSettingsModel
	{
		public string MovementId { get; set; }
		public int AnimationId { get; set; }
		public string PokemonType { get; set; }
		public double Power { get; set; }
		public double AccuracyChance { get; set; }
		public double CriticalChance { get; set; }
		public double StaminaLossScalar { get; set; }
		public int TrainerLevelMin { get; set; }
		public int TrainerLevelMax { get; set; }
		public string VfxName { get; set; }
		public int DurationMs { get; set; }
		public int DamageWindowStartMs { get; set; }
		public int DamageWindowEndMs { get; set; }
		public int EnergyDelta { get; set; }
		public double? HealScalar { get; set; }
	}

	public class IapItemDisplayModel
	{
		public string Sku { get; set; }
		public string Category { get; set; }
		public int SortOrder { get; set; }
		public IList<string> ItemIds { get; set; }
		public IList<int> Counts { get; set; }
	}

	public class MoveSequenceSettingsModel
	{
		public IList<string> Sequence { get; set; }
	}

	public class ItemTemplateModel
	{
		public string TemplateId { get; set; }
		public AvatarCustomizationModel AvatarCustomization { get; set; }
		public BadgeSettingsModel BadgeSettings { get; set; }
		public BattleSettingsModel BattleSettings { get; set; }
		public EncounterSettingsModel EncounterSettings { get; set; }
		public FormSettingsModel FormSettings { get; set; }
		public GymLevelModel GymLevel { get; set; }
		public IapSettingsModel IapSettings { get; set; }
		public ItemSettingsModel ItemSettings { get; set; }
		public PlayerLevelModel PlayerLevel { get; set; }
		public TypeEffectiveModel TypeEffective { get; set; }
		public PokemonUpgradesModel PokemonUpgrades { get; set; }
		public QuestSettingsModel QuestSettings { get; set; }
		public GenderSettingsModel GenderSettings { get; set; }
		public PokemonSettingsModel PokemonSettings { get; set; }
		public MoveSettingsModel MoveSettings { get; set; }
		public IapItemDisplayModel IapItemDisplay { get; set; }
		public CameraModel Camera { get; set; }
		public MoveSequenceSettingsModel MoveSequenceSettings { get; set; }
	}

	public class GameMasterModel
	{
		public IList<ItemTemplateModel> ItemTemplates { get; set; }
		public string TimestampMs { get; set; }
	}
}
