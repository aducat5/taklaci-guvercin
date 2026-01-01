using System;
using System.Collections.Generic;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Shared.DTOs
{
    #region Gene & DNA

    [Serializable]
    public class GeneDto
    {
        public string TraitName = string.Empty;
        public string ExpressedAllele = string.Empty;
        public bool IsDominant;
    }

    [Serializable]
    public class BirdDnaDto
    {
        public GeneDto PrimaryColor = new GeneDto();
        public GeneDto SecondaryColor = new GeneDto();
        public GeneDto Pattern = new GeneDto();
        public GeneDto TailType = new GeneDto();
        public GeneDto CrestType = new GeneDto();
        public Element Element;
    }

    #endregion

    #region Bird Stats

    [Serializable]
    public class BirdStatsDto
    {
        public int Leadership;
        public int Loyalty;
        public int Speed;
        public int GeneticDominance;
        public int TotalPower;
    }

    #endregion

    #region Bird

    [Serializable]
    public class BirdDto
    {
        public string Id;
        public string Name = string.Empty;
        public string OwnerId;
        public BirdState State;
        public BirdRarity Rarity;
        public BirdDnaDto DNA = new BirdDnaDto();
        public BirdStatsDto Stats = new BirdStatsDto();
        public string MotherId;
        public string FatherId;
        public int Generation;
        public int Health;
        public int MaxHealth;
        public int Stamina;
        public int MaxStamina;
    }

    [Serializable]
    public class BirdSummaryDto
    {
        public string Id;
        public string Name = string.Empty;
        public BirdState State;
        public BirdRarity Rarity;
        public Element Element;
        public int Leadership;
        public int Loyalty;
        public int Speed;
        public int GeneticDominance;
        public int TotalPower;
        public int Health;
        public int Stamina;
    }

    #endregion

    #region Player

    [Serializable]
    public class PlayerDto
    {
        public string Id;
        public string Username = string.Empty;
        public int Coins;
        public int PremiumCurrency;
        public int Level;
        public int Experience;
        public int CoopCapacity;
        public PlayerStatsDto Stats = new PlayerStatsDto();
    }

    [Serializable]
    public class PlayerStatsDto
    {
        public int TotalBirdsOwned;
        public int TotalEncountersWon;
        public int TotalEncountersLost;
        public int TotalBirdsLost;
        public int TotalBirdsLooted;
    }

    [Serializable]
    public class PlayerSummaryDto
    {
        public string Id;
        public string Username = string.Empty;
        public int Level;
        public bool IsOnline;
    }

    #endregion

    #region Flight Session

    [Serializable]
    public class FlightSessionDto
    {
        public string Id;
        public string PlayerId;
        public List<BirdSummaryDto> Birds = new List<BirdSummaryDto>();
        public double Latitude;
        public double Longitude;
        public string StartedAt;
        public string EndsAt;
        public bool IsActive;
        public int EncountersCount;
    }

    [Serializable]
    public class StartFlightRequest
    {
        public List<string> BirdIds = new List<string>();
        public double Latitude;
        public double Longitude;
        public int DurationMinutes;
    }

    [Serializable]
    public class FlightPositionUpdate
    {
        public string SessionId;
        public double Latitude;
        public double Longitude;
        public double Altitude;
    }

    #endregion

    #region Encounter

    [Serializable]
    public class EncounterDto
    {
        public string Id;
        public string InitiatorPlayerId;
        public string TargetPlayerId;
        public string InitiatorUsername = string.Empty;
        public string TargetUsername = string.Empty;
        public EncounterState State;
        public string WinnerPlayerId;
        public List<BirdSummaryDto> LootedBirds = new List<BirdSummaryDto>();
        public int CoinsLooted;
        public string CreatedAt;
        public string ResolvedAt;
    }

    [Serializable]
    public class EncounterNotification
    {
        public string EncounterId;
        public string OpponentPlayerId;
        public PlayerSummaryDto OpponentPlayer;
        public List<BirdSummaryDto> OpponentBirds = new List<BirdSummaryDto>();
        public int OpponentTotalPower;
        public int YourTotalPower;
        public int TimeToRespondSeconds;
    }

    [Serializable]
    public class EncounterResultNotification
    {
        public string EncounterId;
        public bool YouWon;
        public List<BirdSummaryDto> BirdsLost = new List<BirdSummaryDto>();
        public List<BirdSummaryDto> BirdsGained = new List<BirdSummaryDto>();
        public int CoinsChange;
        public int ExperienceGained;
    }

    [Serializable]
    public class EncounterStatsDto
    {
        public int TotalEncounters;
        public int Wins;
        public int Losses;
        public double WinRate;
        public int BirdsLost;
        public int BirdsLooted;
    }

    [Serializable]
    public class EncounterPreviewDto
    {
        public string Session1Id;
        public string Session2Id;
        public int Power1;
        public int Power2;
        public double WinChance1;
        public double WinChance2;
        public List<BirdSummaryDto> Session1Birds = new List<BirdSummaryDto>();
        public List<BirdSummaryDto> Session2Birds = new List<BirdSummaryDto>();
    }

    [Serializable]
    public class EncounterPreviewRequest
    {
        public string Session1Id;
        public string Session2Id;
    }

    #endregion

    #region Auth

    [Serializable]
    public class LoginRequest
    {
        public string Email = string.Empty;
        public string Password = string.Empty;
    }

    [Serializable]
    public class RegisterRequest
    {
        public string Username = string.Empty;
        public string Email = string.Empty;
        public string Password = string.Empty;
    }

    [Serializable]
    public class AuthResponse
    {
        public bool Success;
        public string Token = string.Empty;
        public string RefreshToken = string.Empty;
        public PlayerDto Player;
        public string Error;
    }

    [Serializable]
    public class RefreshTokenRequest
    {
        public string RefreshToken = string.Empty;
    }

    #endregion
}
