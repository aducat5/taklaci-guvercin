using Microsoft.AspNetCore.Mvc;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Domain.ValueObjects;
using TaklaciGuvercin.Shared.Common;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BreedingController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random = new();

    public BreedingController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<Result<BreedingPreviewDto>>> PreviewBreeding([FromBody] BreedingRequest request)
    {
        var mother = await _unitOfWork.Birds.GetByIdAsync(request.MotherId);
        var father = await _unitOfWork.Birds.GetByIdAsync(request.FatherId);

        if (mother == null || father == null)
            return NotFound(Result.Failure<BreedingPreviewDto>("One or both birds not found"));

        if (mother.OwnerId != father.OwnerId)
            return BadRequest(Result.Failure<BreedingPreviewDto>("Both birds must belong to the same player"));

        var preview = new BreedingPreviewDto
        {
            CanBreed = mother.CanBreed() && father.CanBreed(),
            CannotBreedReason = GetCannotBreedReason(mother, father),
            PossibleTraits = GetPossibleTraits(mother, father),
            MutationChance = (mother.DNA.MutationFactor + father.DNA.MutationFactor) / 2,
            PossibleRarities = GetPossibleRarities(mother, father)
        };

        return Ok(Result.Success(preview));
    }

    [HttpPost]
    public async Task<ActionResult<Result<BreedingResultDto>>> Breed([FromBody] BreedingRequest request)
    {
        var mother = await _unitOfWork.Birds.GetByIdAsync(request.MotherId);
        var father = await _unitOfWork.Birds.GetByIdAsync(request.FatherId);

        if (mother == null || father == null)
            return NotFound(Result.Failure<BreedingResultDto>("One or both birds not found"));

        if (mother.OwnerId != father.OwnerId)
            return BadRequest(Result.Failure<BreedingResultDto>("Both birds must belong to the same player"));

        if (!mother.CanBreed())
            return BadRequest(Result.Failure<BreedingResultDto>($"{mother.Name} is not ready for breeding"));

        if (!father.CanBreed())
            return BadRequest(Result.Failure<BreedingResultDto>($"{father.Name} is not ready for breeding"));

        // Check coop capacity
        var player = await _unitOfWork.Players.GetByIdAsync(mother.OwnerId);
        if (player == null)
            return BadRequest(Result.Failure<BreedingResultDto>("Player not found"));

        var birdCount = await _unitOfWork.Birds.GetOwnerBirdCountAsync(mother.OwnerId);
        if (birdCount >= player.CoopCapacity)
            return BadRequest(Result.Failure<BreedingResultDto>("Coop is full"));

        // Perform breeding with Mendelian genetics
        var (offspringDna, inheritedTraits, mutations) = BreedDNA(mother.DNA, father.DNA);
        var offspringStats = BreedStats(mother.Stats, father.Stats);
        var offspringRarity = DetermineRarity(mother.Rarity, father.Rarity, mutations.Count > 0);

        var offspring = Bird.Create(
            GenerateOffspringName(mother.Name, father.Name),
            mother.OwnerId,
            offspringDna,
            offspringStats,
            offspringRarity,
            mother.Id,
            father.Id,
            Math.Max(mother.Generation, father.Generation) + 1
        );

        // Apply stamina cost to parents
        mother.ConsumeStamina(30);
        father.ConsumeStamina(30);

        await _unitOfWork.Birds.AddAsync(offspring);
        _unitOfWork.Birds.Update(mother);
        _unitOfWork.Birds.Update(father);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(new BreedingResultDto
        {
            Success = true,
            Offspring = MapToDto(offspring),
            Message = mutations.Count > 0 ? "A mutation occurred!" : "Breeding successful!",
            InheritedTraits = inheritedTraits,
            Mutations = mutations
        }));
    }

    private (BirdDNA dna, List<string> inheritedTraits, List<string> mutations) BreedDNA(BirdDNA mother, BirdDNA father)
    {
        var inheritedTraits = new List<string>();
        var mutations = new List<string>();
        var mutationChance = (mother.MutationFactor + father.MutationFactor) / 2;

        // Mendelian inheritance for each gene
        var primaryColor = InheritGene(mother.PrimaryColor, father.PrimaryColor, ref inheritedTraits, ref mutations, mutationChance);
        var secondaryColor = InheritGene(mother.SecondaryColor, father.SecondaryColor, ref inheritedTraits, ref mutations, mutationChance);
        var pattern = InheritGene(mother.Pattern, father.Pattern, ref inheritedTraits, ref mutations, mutationChance);
        var tailType = InheritGene(mother.TailType, father.TailType, ref inheritedTraits, ref mutations, mutationChance);
        var crestType = InheritGene(mother.CrestType, father.CrestType, ref inheritedTraits, ref mutations, mutationChance);
        var elementGene = InheritGene(mother.ElementGene, father.ElementGene, ref inheritedTraits, ref mutations, mutationChance);
        var hiddenTrait1 = InheritGene(mother.HiddenTrait1, father.HiddenTrait1, ref inheritedTraits, ref mutations, mutationChance);
        var hiddenTrait2 = InheritGene(mother.HiddenTrait2, father.HiddenTrait2, ref inheritedTraits, ref mutations, mutationChance);

        // Determine element from element gene
        var element = DetermineElement(elementGene, mother.Element, father.Element);

        return (BirdDNA.Create(
            primaryColor,
            secondaryColor,
            pattern,
            tailType,
            crestType,
            elementGene,
            element,
            hiddenTrait1,
            hiddenTrait2,
            Math.Min(mutationChance * 1.1f, 0.3f) // Slightly increase mutation chance for offspring
        ), inheritedTraits, mutations);
    }

    private Gene InheritGene(Gene mother, Gene father, ref List<string> inherited, ref List<string> mutations, float mutationChance)
    {
        // Select one allele from each parent (Mendelian)
        var allele1 = _random.Next(2) == 0 ? mother.Allele1 : mother.Allele2;
        var allele2 = _random.Next(2) == 0 ? father.Allele1 : father.Allele2;

        // Check for mutation
        if (_random.NextDouble() < mutationChance)
        {
            mutations.Add($"{mother.TraitName} mutated!");
            // For simplicity, swap dominance type
            var newType = mother.Type == GeneType.Dominant ? GeneType.Recessive : GeneType.Dominant;
            return Gene.Create(mother.TraitName, allele1, allele2, newType);
        }

        inherited.Add($"{mother.TraitName}: {allele1}/{allele2}");

        // Determine dominance based on parent genes
        var type = (mother.Type == GeneType.Dominant || father.Type == GeneType.Dominant)
            ? GeneType.Dominant
            : GeneType.Recessive;

        return Gene.Create(mother.TraitName, allele1, allele2, type);
    }

    private BirdStats BreedStats(BirdStats mother, BirdStats father)
    {
        // Stats inheritance with some variation
        var variance = 10;

        return BirdStats.Create(
            GetInheritedStat(mother.Leadership, father.Leadership, variance),
            GetInheritedStat(mother.Loyalty, father.Loyalty, variance),
            GetInheritedStat(mother.Speed, father.Speed, variance),
            GetInheritedStat(mother.GeneticDominance, father.GeneticDominance, variance)
        );
    }

    private int GetInheritedStat(int motherStat, int fatherStat, int variance)
    {
        var average = (motherStat + fatherStat) / 2;
        var variation = _random.Next(-variance, variance + 1);
        return Math.Clamp(average + variation, BirdStats.MinStatValue, BirdStats.MaxStatValue);
    }

    private Element DetermineElement(Gene elementGene, Element motherElement, Element fatherElement)
    {
        // Element matrix logic
        if (motherElement == fatherElement)
            return motherElement;

        // Different elements can combine to create new ones
        var elements = new[] { motherElement, fatherElement }.OrderBy(e => e).ToArray();

        // Fire + Ice = None (cancel out)
        if (elements.Contains(Element.Fire) && elements.Contains(Element.Ice))
            return _random.Next(2) == 0 ? Element.None : Element.Wind;

        // Fire + Wind = Fire (fire spreads)
        if (elements.Contains(Element.Fire) && elements.Contains(Element.Wind))
            return Element.Fire;

        // Ice + Wind = Ice
        if (elements.Contains(Element.Ice) && elements.Contains(Element.Wind))
            return Element.Ice;

        // Emerald + any = chance of Emerald
        if (elements.Contains(Element.Emerald))
            return _random.Next(3) == 0 ? Element.Emerald : elements.First(e => e != Element.Emerald);

        // Default: inherit from one parent randomly
        return _random.Next(2) == 0 ? motherElement : fatherElement;
    }

    private BirdRarity DetermineRarity(BirdRarity mother, BirdRarity father, bool hasMutation)
    {
        var baseRarity = (int)Math.Max((int)mother, (int)father);

        // Chance to upgrade rarity
        if (hasMutation && _random.Next(100) < 20)
            baseRarity = Math.Min(baseRarity + 1, (int)BirdRarity.Mythical);

        // Small chance to match or exceed parent rarity
        if (_random.Next(100) < 10)
            baseRarity = Math.Min(baseRarity + 1, (int)BirdRarity.Mythical);

        return (BirdRarity)baseRarity;
    }

    private string GenerateOffspringName(string motherName, string fatherName)
    {
        var prefixes = new[] { "Little", "Baby", "Young", "Jr.", "Mini" };
        var baseName = _random.Next(2) == 0 ? motherName : fatherName;
        return $"{prefixes[_random.Next(prefixes.Length)]} {baseName}";
    }

    private static string? GetCannotBreedReason(Bird mother, Bird father)
    {
        if (mother.State != BirdState.InCoop) return $"{mother.Name} must be in coop";
        if (father.State != BirdState.InCoop) return $"{father.Name} must be in coop";
        if (mother.Health <= 50) return $"{mother.Name} needs more health";
        if (father.Health <= 50) return $"{father.Name} needs more health";
        if (mother.Stamina <= 30) return $"{mother.Name} needs more stamina";
        if (father.Stamina <= 30) return $"{father.Name} needs more stamina";
        return null;
    }

    private List<PossibleTraitDto> GetPossibleTraits(Bird mother, Bird father)
    {
        var traits = new List<PossibleTraitDto>();

        AddTraitPossibility(traits, "PrimaryColor", mother.DNA.PrimaryColor, father.DNA.PrimaryColor);
        AddTraitPossibility(traits, "SecondaryColor", mother.DNA.SecondaryColor, father.DNA.SecondaryColor);
        AddTraitPossibility(traits, "Element", mother.DNA.Element.ToString(), father.DNA.Element.ToString());

        return traits;
    }

    private void AddTraitPossibility(List<PossibleTraitDto> traits, string name, Gene mother, Gene father)
    {
        var alleles = new[] { mother.Allele1, mother.Allele2, father.Allele1, father.Allele2 }
            .Distinct()
            .ToList();

        foreach (var allele in alleles)
        {
            traits.Add(new PossibleTraitDto
            {
                TraitName = name,
                PossibleValue = allele,
                Probability = 1f / alleles.Count
            });
        }
    }

    private void AddTraitPossibility(List<PossibleTraitDto> traits, string name, string mother, string father)
    {
        traits.Add(new PossibleTraitDto { TraitName = name, PossibleValue = mother, Probability = 0.5f });
        if (mother != father)
            traits.Add(new PossibleTraitDto { TraitName = name, PossibleValue = father, Probability = 0.5f });
    }

    private List<string> GetPossibleRarities(Bird mother, Bird father)
    {
        var minRarity = (int)Math.Min((int)mother.Rarity, (int)father.Rarity);
        var maxRarity = Math.Min((int)Math.Max((int)mother.Rarity, (int)father.Rarity) + 1, (int)BirdRarity.Mythical);

        return Enumerable.Range(minRarity, maxRarity - minRarity + 1)
            .Select(r => ((BirdRarity)r).ToString())
            .ToList();
    }

    private static BirdDto MapToDto(Bird bird) => new()
    {
        Id = bird.Id,
        Name = bird.Name,
        OwnerId = bird.OwnerId,
        State = (Shared.Enums.BirdStateDto)bird.State,
        Rarity = (Shared.Enums.BirdRarityDto)bird.Rarity,
        DNA = new BirdDnaDto
        {
            PrimaryColor = new GeneDto
            {
                TraitName = bird.DNA.PrimaryColor.TraitName,
                ExpressedAllele = bird.DNA.PrimaryColor.GetExpressedAllele(),
                IsDominant = bird.DNA.PrimaryColor.IsDominant
            },
            SecondaryColor = new GeneDto
            {
                TraitName = bird.DNA.SecondaryColor.TraitName,
                ExpressedAllele = bird.DNA.SecondaryColor.GetExpressedAllele(),
                IsDominant = bird.DNA.SecondaryColor.IsDominant
            },
            Pattern = new GeneDto
            {
                TraitName = bird.DNA.Pattern.TraitName,
                ExpressedAllele = bird.DNA.Pattern.GetExpressedAllele(),
                IsDominant = bird.DNA.Pattern.IsDominant
            },
            TailType = new GeneDto
            {
                TraitName = bird.DNA.TailType.TraitName,
                ExpressedAllele = bird.DNA.TailType.GetExpressedAllele(),
                IsDominant = bird.DNA.TailType.IsDominant
            },
            CrestType = new GeneDto
            {
                TraitName = bird.DNA.CrestType.TraitName,
                ExpressedAllele = bird.DNA.CrestType.GetExpressedAllele(),
                IsDominant = bird.DNA.CrestType.IsDominant
            },
            Element = (Shared.Enums.ElementDto)bird.DNA.Element
        },
        Stats = new BirdStatsDto
        {
            Leadership = bird.Stats.Leadership,
            Loyalty = bird.Stats.Loyalty,
            Speed = bird.Stats.Speed,
            GeneticDominance = bird.Stats.GeneticDominance,
            TotalPower = bird.Stats.GetTotalPower()
        },
        MotherId = bird.MotherId,
        FatherId = bird.FatherId,
        Generation = bird.Generation,
        Health = bird.Health,
        MaxHealth = bird.MaxHealth,
        Stamina = bird.Stamina,
        MaxStamina = bird.MaxStamina
    };
}
