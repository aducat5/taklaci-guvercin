namespace TaklaciGuvercin.Shared.DTOs;

public class BreedingRequest
{
    public Guid MotherId { get; set; }
    public Guid FatherId { get; set; }
}

public class BreedingResultDto
{
    public bool Success { get; set; }
    public BirdDto? Offspring { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> InheritedTraits { get; set; } = new();
    public List<string> Mutations { get; set; } = new();
}

public class BreedingPreviewDto
{
    public bool CanBreed { get; set; }
    public string? CannotBreedReason { get; set; }
    public List<PossibleTraitDto> PossibleTraits { get; set; } = new();
    public float MutationChance { get; set; }
    public List<string> PossibleRarities { get; set; } = new();
}

public class PossibleTraitDto
{
    public string TraitName { get; set; } = string.Empty;
    public string PossibleValue { get; set; } = string.Empty;
    public float Probability { get; set; }
}
