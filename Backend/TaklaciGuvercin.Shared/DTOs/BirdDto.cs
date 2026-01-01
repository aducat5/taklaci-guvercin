using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Shared.DTOs;

public class BirdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public BirdStateDto State { get; set; }
    public BirdRarityDto Rarity { get; set; }

    public BirdDnaDto DNA { get; set; } = new();
    public BirdStatsDto Stats { get; set; } = new();

    public Guid? MotherId { get; set; }
    public Guid? FatherId { get; set; }
    public int Generation { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
}

public class BirdSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BirdStateDto State { get; set; }
    public BirdRarityDto Rarity { get; set; }
    public ElementDto Element { get; set; }
    public int Leadership { get; set; }
    public int Loyalty { get; set; }
    public int Speed { get; set; }
    public int GeneticDominance { get; set; }
    public int TotalPower { get; set; }
    public int Health { get; set; }
    public int Stamina { get; set; }
}
