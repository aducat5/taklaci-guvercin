using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Shared.DTOs;

public class EncounterDto
{
    public Guid Id { get; set; }
    public Guid InitiatorPlayerId { get; set; }
    public Guid TargetPlayerId { get; set; }
    public string InitiatorUsername { get; set; } = string.Empty;
    public string TargetUsername { get; set; } = string.Empty;
    public EncounterStateDto State { get; set; }
    public Guid? WinnerPlayerId { get; set; }
    public List<BirdSummaryDto> LootedBirds { get; set; } = new();
    public int CoinsLooted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class EncounterNotification
{
    public Guid EncounterId { get; set; }
    public PlayerSummaryDto OpponentPlayer { get; set; } = new();
    public List<BirdSummaryDto> OpponentBirds { get; set; } = new();
    public int OpponentTotalPower { get; set; }
    public int YourTotalPower { get; set; }
    public int TimeToRespondSeconds { get; set; }
}

public class EncounterResultNotification
{
    public Guid EncounterId { get; set; }
    public bool YouWon { get; set; }
    public List<BirdSummaryDto> BirdsLost { get; set; } = new();
    public List<BirdSummaryDto> BirdsGained { get; set; } = new();
    public int CoinsChange { get; set; }
    public int ExperienceGained { get; set; }
}
