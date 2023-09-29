using System.Collections.Generic;

namespace LoLMatchStatistics
{
    internal class RiotApiMatch //class to manage riot api json file
    {
        public MetaData MetaData { get; set; }
        public Info Info { get; set; }
    }

    class MetaData
    {
        public string DataVersion { get; set; }
        public string MatchId { get; set; }
        public List<string> Participants { get; set; }
    }

    class Info
    {
        public long GameCreation { get; set; }
        public long GameDuration { get; set; }
        public long GameEndTimestamp { get; set; }
        public long GameId { get; set; }
        public string GameMode { get; set; }
        public string GameName { get; set; }
        public long GameStartTimestamp { get; set; }
        public string GameType { get; set; }
        public string GameVersion { get; set; }
        public int MapId { get; set; }
        public List<Participant> Participants { get; set; }
        public string PlatformId { get; set; }
        public int QueueId { get; set; }
        public List<Team> Teams { get; set; }
        public string TournamentCode { get; set; }

    }

    public class Participant
    {
        public int Assists { get; set; }
        public int BaronKills { get; set; }
        public int BountyLevel { get; set; }
        public int ChampExperience { get; set; }
        public int ChampLevel { get; set; }
        public int ChampionId { get; set; }
        public string ChampionName { get; set; }
        public int ChampionTransform { get; set; }
        public int ConsumablesPurchased { get; set; }
        public int DamageDealtToBuildings { get; set; }
        public int DamageDealtToObjectives { get; set; }
        public int DamageDealtToTurrets { get; set; }
        public int DamageSelfMitigated { get; set; }
        public int Deaths { get; set; }
        public int DetectorWardsPlaced { get; set; }
        public int DoubleKills { get; set; }
        public int DragonKills { get; set; }
        public bool FirstBloodAssist { get; set; }
        public bool FirstBloodKill { get; set; }
        public bool FirstTowerAssist { get; set; }
        public bool FirstTowerKill { get; set; }
        public bool GameEndedInEarlySurrender { get; set; }
        public bool GameEndedInSurrender { get; set; }
        public int GoldEarned { get; set; }
        public int GoldSpent { get; set; }
        public string IndividualPosition { get; set; }
        public int InhibitorKills { get; set; }
        public int InhibitorTakedowns { get; set; }
        public int InhibitorsLost { get; set; }
        public int Item0 { get; set; }
        public int Item1 { get; set; }
        public int Item2 { get; set; }
        public int Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }
        public int Item6 { get; set; }
        public int ItemsPurchased { get; set; }
        public int KillingSprees { get; set; }
        public int Kills { get; set; }
        public string Lane { get; set; }
        public int LargestCriticalStrike { get; set; }
        public int LargestKillingSpree { get; set; }
        public int LargestMultiKill { get; set; }
        public int LongestTimeSpentLiving { get; set; }
        public int MagicDamageDealt { get; set; }
        public int MagicDamageDealtToChampions { get; set; }
        public int MagicDamageTaken { get; set; }
        public int NeutralMinionsKilled { get; set; }
        public int NexusKills { get; set; }
        public int NexusTakedowns { get; set; }
        public int NexusLost { get; set; }
        public int ObjectivesStolen { get; set; }
        public int ObjectivesStolenAssists { get; set; }
        public int ParticipantId { get; set; }
        public int PentaKills { get; set; }
        public Perks Perks { get; set; }
        public int PhysicalDamageDealt { get; set; }
        public int PhysicalDamageDealtToChampions { get; set; }
        public int PhysicalDamageTaken { get; set; }
        public int ProfileIcon { get; set; }
        public string Puuid { get; set; }
        public int QuadraKills { get; set; }
        public string RiotIdName { get; set; }
        public string RiotIdTagline { get; set; }
        public string Role { get; set; }
        public int SightWardsBoughtInGame { get; set; }
        public int Spell1Casts { get; set; }
        public int Spell2Casts { get; set; }
        public int Spell3Casts { get; set; }
        public int Spell4Casts { get; set; }
        public int Summoner1Casts { get; set; }
        public int Summoner1Id { get; set; }
        public int Summoner2Casts { get; set; }
        public int Summoner2Id { get; set; }
        public string SummonerId { get; set; }
        public int SummonerLevel { get; set; }
        public string SummonerName { get; set; }
        public bool TeamEarlySurrendered { get; set; }
        public int TeamId { get; set; }
        public string TeamPosition { get; set; }
        public int TimeCCingOthers { get; set; }
        public int TimePlayed { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageShieldedOnTeammates { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TotalHeal { get; set; }
        public int TotalHealsOnTeammates { get; set; }
        public int TotalMinionsKilled { get; set; }
        public int TotalTimeCCDealt { get; set; }
        public int TotalTimeSpentDead { get; set; }
        public int TotalUnitsHealed { get; set; }
        public int TripleKills { get; set; }
        public int TrueDamageDealt { get; set; }
        public int TrueDamageDealtToChampions { get; set; }
        public int TrueDamageTaken { get; set; }
        public int TurretKills { get; set; }
        public int TurretTakedowns { get; set; }
        public int TurretsLost { get; set; }
        public int UnrealKills { get; set; }
        public int VisionScore { get; set; }
        public int VisionWardsBoughtInGame { get; set; }
        public int WardsKilled { get; set; }
        public int WardsPlaced { get; set; }
        public bool Win { get; set; }
    }


    public class Perks
    {
        public PerkStats StatPerks { get; set; }
        public List<PerkStyle> Styles { get; set; }
    }

    public class PerkStats
    {
        public int Defense { get; set; }
        public int Flex { get; set; }
        public int Offense { get; set; }
    }

    public class PerkStyle
    {
        public string Description { get; set; }
        public List<PerkStyleSelection> Selections { get; set; }
        public int Style { get; set; }
    }

    public class PerkStyleSelection
    {
        public int Perk { get; set; }
        public int Var1 { get; set; }
        public int Var2 { get; set; }
        public int Var3 { get; set; }
    }

    public class Team
    {
        public List<Ban> Bans { get; set; }
        public Objectives Objectives { get; set; }
        public int TeamId { get; set; }
        public bool Win { get; set; }
    }

    public class Ban
    {
        public int ChampionId { get; set; }
        public int PickTurn { get; set; }
    }

    public class Objectives
    {
        public Objective Baron { get; set; }
        public Objective Champion { get; set; }
        public Objective Dragon { get; set; }
        public Objective Inhibitor { get; set; }
        public Objective RiftHerald { get; set; }
        public Objective Tower { get; set; }
    }

    public class Objective
    {
        public bool First { get; set; }
        public int Kills { get; set; }
    }

}
