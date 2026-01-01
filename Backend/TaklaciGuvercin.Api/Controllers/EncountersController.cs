using Microsoft.AspNetCore.Mvc;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Shared.Common;
using TaklaciGuvercin.Shared.DTOs;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EncountersController : ControllerBase
{
    private readonly IEncounterService _encounterService;
    private readonly IFlightSessionRepository _flightRepository;
    private readonly IBirdRepository _birdRepository;
    private readonly IPlayerRepository _playerRepository;

    public EncountersController(
        IEncounterService encounterService,
        IFlightSessionRepository flightRepository,
        IBirdRepository birdRepository,
        IPlayerRepository playerRepository)
    {
        _encounterService = encounterService;
        _flightRepository = flightRepository;
        _birdRepository = birdRepository;
        _playerRepository = playerRepository;
    }

    [HttpGet("{encounterId}")]
    public async Task<ActionResult<Result<EncounterDto>>> GetEncounter(Guid encounterId)
    {
        var encounter = await _encounterService.GetByIdAsync(encounterId);
        if (encounter == null)
            return NotFound(Result<EncounterDto>.Failure("Encounter not found"));

        var dto = await MapToDto(encounter);
        return Ok(Result<EncounterDto>.Success(dto));
    }

    [HttpGet("player/{playerId}/active")]
    public async Task<ActionResult<Result<List<EncounterDto>>>> GetActiveEncounters(Guid playerId)
    {
        var encounters = await _encounterService.GetActiveEncountersForPlayerAsync(playerId);
        var dtos = new List<EncounterDto>();

        foreach (var encounter in encounters)
        {
            dtos.Add(await MapToDto(encounter));
        }

        return Ok(Result<List<EncounterDto>>.Success(dtos));
    }

    [HttpGet("player/{playerId}/history")]
    public async Task<ActionResult<Result<List<EncounterDto>>>> GetEncounterHistory(Guid playerId, [FromQuery] int count = 10)
    {
        var encounters = await _encounterService.GetEncounterHistoryAsync(playerId, count);
        var dtos = new List<EncounterDto>();

        foreach (var encounter in encounters)
        {
            dtos.Add(await MapToDto(encounter));
        }

        return Ok(Result<List<EncounterDto>>.Success(dtos));
    }

    [HttpGet("player/{playerId}/stats")]
    public async Task<ActionResult<Result<EncounterStatsDto>>> GetPlayerStats(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
            return NotFound(Result<EncounterStatsDto>.Failure("Player not found"));

        var stats = new EncounterStatsDto
        {
            TotalEncounters = player.TotalEncountersWon + player.TotalEncountersLost,
            Wins = player.TotalEncountersWon,
            Losses = player.TotalEncountersLost,
            WinRate = player.TotalEncountersWon + player.TotalEncountersLost > 0
                ? (double)player.TotalEncountersWon / (player.TotalEncountersWon + player.TotalEncountersLost)
                : 0,
            BirdsLost = player.TotalBirdsLost,
            BirdsLooted = player.TotalBirdsLooted
        };

        return Ok(Result<EncounterStatsDto>.Success(stats));
    }

    [HttpPost("{encounterId}/resolve")]
    public async Task<ActionResult<Result<EncounterDto>>> ResolveEncounter(Guid encounterId)
    {
        var encounter = await _encounterService.ResolveEncounterAsync(encounterId);
        if (encounter == null)
            return NotFound(Result<EncounterDto>.Failure("Encounter not found or already resolved"));

        var dto = await MapToDto(encounter);
        return Ok(Result<EncounterDto>.Success(dto));
    }

    [HttpPost("{encounterId}/cancel")]
    public async Task<ActionResult<Result<EncounterDto>>> CancelEncounter(Guid encounterId)
    {
        var encounter = await _encounterService.CancelEncounterAsync(encounterId);
        if (encounter == null)
            return NotFound(Result<EncounterDto>.Failure("Encounter not found or cannot be cancelled"));

        var dto = await MapToDto(encounter);
        return Ok(Result<EncounterDto>.Success(dto));
    }

    [HttpPost("preview")]
    public async Task<ActionResult<Result<EncounterPreviewDto>>> PreviewEncounter([FromBody] EncounterPreviewRequest request)
    {
        var session1 = await _flightRepository.GetByIdAsync(request.Session1Id);
        var session2 = await _flightRepository.GetByIdAsync(request.Session2Id);

        if (session1 == null || session2 == null)
            return NotFound(Result<EncounterPreviewDto>.Failure("One or both sessions not found"));

        var preview = await _encounterService.PreviewEncounterAsync(session1, session2);

        var dto = new EncounterPreviewDto
        {
            Session1Id = preview.Session1Id,
            Session2Id = preview.Session2Id,
            Power1 = preview.Power1,
            Power2 = preview.Power2,
            WinChance1 = preview.WinChance1,
            WinChance2 = preview.WinChance2,
            Session1Birds = preview.Session1Birds.Select(b => MapToBirdSummary(b)).ToList(),
            Session2Birds = preview.Session2Birds.Select(b => MapToBirdSummary(b)).ToList()
        };

        return Ok(Result<EncounterPreviewDto>.Success(dto));
    }

    private async Task<EncounterDto> MapToDto(Domain.Entities.Encounter encounter)
    {
        var initiatorPlayer = await _playerRepository.GetByIdAsync(encounter.InitiatorPlayerId);
        var targetPlayer = await _playerRepository.GetByIdAsync(encounter.TargetPlayerId);

        var lootedBirds = new List<BirdSummaryDto>();
        foreach (var birdId in encounter.LootedBirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null)
            {
                lootedBirds.Add(MapToBirdSummary(bird));
            }
        }

        return new EncounterDto
        {
            Id = encounter.Id,
            InitiatorPlayerId = encounter.InitiatorPlayerId,
            TargetPlayerId = encounter.TargetPlayerId,
            InitiatorUsername = initiatorPlayer?.Username ?? "Unknown",
            TargetUsername = targetPlayer?.Username ?? "Unknown",
            State = (EncounterStateDto)encounter.State,
            WinnerPlayerId = encounter.WinnerPlayerId,
            LootedBirds = lootedBirds,
            CoinsLooted = encounter.CoinsLooted,
            CreatedAt = encounter.CreatedAt,
            ResolvedAt = encounter.ResolvedAt
        };
    }

    private static BirdSummaryDto MapToBirdSummary(Domain.Entities.Bird bird)
    {
        return new BirdSummaryDto
        {
            Id = bird.Id,
            Name = bird.Name,
            Rarity = (BirdRarityDto)bird.Rarity,
            State = (BirdStateDto)bird.State,
            Element = (ElementDto)bird.DNA.Element,
            Leadership = bird.Stats.Leadership,
            Loyalty = bird.Stats.Loyalty,
            Speed = bird.Stats.Speed,
            GeneticDominance = bird.Stats.GeneticDominance,
            TotalPower = bird.Stats.GetTotalPower(),
            Health = bird.Health,
            Stamina = bird.Stamina
        };
    }
}

public class EncounterPreviewRequest
{
    public Guid Session1Id { get; set; }
    public Guid Session2Id { get; set; }
}

public class EncounterStatsDto
{
    public int TotalEncounters { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
    public int BirdsLost { get; set; }
    public int BirdsLooted { get; set; }
}

public class EncounterPreviewDto
{
    public Guid Session1Id { get; set; }
    public Guid Session2Id { get; set; }
    public int Power1 { get; set; }
    public int Power2 { get; set; }
    public double WinChance1 { get; set; }
    public double WinChance2 { get; set; }
    public List<BirdSummaryDto> Session1Birds { get; set; } = new();
    public List<BirdSummaryDto> Session2Birds { get; set; } = new();
}
