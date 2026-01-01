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
public class BirdsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BirdsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<Result<IEnumerable<BirdSummaryDto>>>> GetPlayerBirds(Guid playerId)
    {
        var birds = await _unitOfWork.Birds.GetByOwnerIdAsync(playerId);
        var summaries = birds.Select(MapToSummary);
        return Ok(Result.Success(summaries));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<BirdDto>>> GetBird(Guid id)
    {
        var bird = await _unitOfWork.Birds.GetByIdAsync(id);
        if (bird == null)
            return NotFound(Result.Failure<BirdDto>("Bird not found"));

        return Ok(Result.Success(MapToDto(bird)));
    }

    [HttpGet("{id}/lineage")]
    public async Task<ActionResult<Result<IEnumerable<BirdSummaryDto>>>> GetLineage(Guid id, [FromQuery] int generations = 3)
    {
        var lineage = await _unitOfWork.Birds.GetLineageAsync(id, generations);
        var summaries = lineage.Select(MapToSummary);
        return Ok(Result.Success(summaries));
    }

    [HttpPost]
    public async Task<ActionResult<Result<BirdDto>>> CreateBird([FromBody] CreateBirdRequest request)
    {
        var player = await _unitOfWork.Players.GetByIdAsync(request.OwnerId);
        if (player == null)
            return BadRequest(Result.Failure<BirdDto>("Player not found"));

        var birdCount = await _unitOfWork.Birds.GetOwnerBirdCountAsync(request.OwnerId);
        if (birdCount >= player.CoopCapacity)
            return BadRequest(Result.Failure<BirdDto>("Coop is full"));

        var bird = Bird.Create(
            request.Name,
            request.OwnerId,
            BirdDNA.CreateDefault(),
            BirdStats.CreateRandom()
        );

        await _unitOfWork.Birds.AddAsync(bird);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBird), new { id = bird.Id }, Result.Success(MapToDto(bird)));
    }

    [HttpPut("{id}/feed")]
    public async Task<ActionResult<Result<BirdDto>>> FeedBird(Guid id)
    {
        var bird = await _unitOfWork.Birds.GetByIdAsync(id);
        if (bird == null)
            return NotFound(Result.Failure<BirdDto>("Bird not found"));

        bird.Feed();
        _unitOfWork.Birds.Update(bird);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(MapToDto(bird)));
    }

    [HttpPut("{id}/rename")]
    public async Task<ActionResult<Result<BirdDto>>> RenameBird(Guid id, [FromBody] RenameBirdRequest request)
    {
        var bird = await _unitOfWork.Birds.GetByIdAsync(id);
        if (bird == null)
            return NotFound(Result.Failure<BirdDto>("Bird not found"));

        bird.Rename(request.NewName);
        _unitOfWork.Birds.Update(bird);
        await _unitOfWork.SaveChangesAsync();

        return Ok(Result.Success(MapToDto(bird)));
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
            PrimaryColor = MapGene(bird.DNA.PrimaryColor),
            SecondaryColor = MapGene(bird.DNA.SecondaryColor),
            Pattern = MapGene(bird.DNA.Pattern),
            TailType = MapGene(bird.DNA.TailType),
            CrestType = MapGene(bird.DNA.CrestType),
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

    private static BirdSummaryDto MapToSummary(Bird bird) => new()
    {
        Id = bird.Id,
        Name = bird.Name,
        State = (Shared.Enums.BirdStateDto)bird.State,
        Rarity = (Shared.Enums.BirdRarityDto)bird.Rarity,
        Element = (Shared.Enums.ElementDto)bird.DNA.Element,
        TotalPower = bird.Stats.GetTotalPower()
    };

    private static GeneDto MapGene(Gene gene) => new()
    {
        TraitName = gene.TraitName,
        ExpressedAllele = gene.GetExpressedAllele(),
        IsDominant = gene.IsDominant
    };
}

public class CreateBirdRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
}

public class RenameBirdRequest
{
    public string NewName { get; set; } = string.Empty;
}
