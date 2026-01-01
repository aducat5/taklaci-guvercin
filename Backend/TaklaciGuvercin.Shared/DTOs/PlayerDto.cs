namespace TaklaciGuvercin.Shared.DTOs;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Coins { get; set; }
    public int PremiumCurrency { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public int CoopCapacity { get; set; }
    public PlayerStatsDto Stats { get; set; } = new();
}

public class PlayerStatsDto
{
    public int TotalBirdsOwned { get; set; }
    public int TotalEncountersWon { get; set; }
    public int TotalEncountersLost { get; set; }
    public int TotalBirdsLost { get; set; }
    public int TotalBirdsLooted { get; set; }
}

public class PlayerSummaryDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool IsOnline { get; set; }
}
