using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Infrastructure.Hubs;
using TaklaciGuvercin.Shared.Common;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AirspaceHub, IAirspaceClient> _hubContext;

    public FlightsController(IUnitOfWork unitOfWork, IHubContext<AirspaceHub, IAirspaceClient> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }

    [HttpGet("player/{playerId}")]
    public async Task<ActionResult<Result<FlightSessionDto?>>> GetActiveSession(Guid playerId)
    {
        var session = await _unitOfWork.FlightSessions.GetActiveByPlayerIdAsync(playerId);
        if (session == null)
            return Ok(Result.Success<FlightSessionDto?>(null));

        var birds = await _unitOfWork.Birds.GetByIdsAsync(session.BirdIds);
        return Ok(Result.Success<FlightSessionDto?>(MapToDto(session, birds)));
    }

    [HttpPost("start")]
    public async Task<ActionResult<Result<FlightSessionDto>>> StartFlight([FromBody] StartFlightRequest request)
    {
        // Validate player exists
        var player = await _unitOfWork.Players.GetByIdAsync(request.BirdIds.FirstOrDefault());

        // Check if player already has an active flight
        var existingSession = await _unitOfWork.FlightSessions.GetActiveByPlayerIdAsync(request.BirdIds.FirstOrDefault());

        // Get and validate birds
        var birds = (await _unitOfWork.Birds.GetByIdsAsync(request.BirdIds)).ToList();
        if (birds.Count == 0)
            return BadRequest(Result.Failure<FlightSessionDto>("No valid birds selected"));

        var ownerId = birds.First().OwnerId;

        // Verify all birds belong to same owner and are ready
        foreach (var bird in birds)
        {
            if (bird.OwnerId != ownerId)
                return BadRequest(Result.Failure<FlightSessionDto>("All birds must belong to the same player"));

            if (!bird.IsReadyForFlight())
                return BadRequest(Result.Failure<FlightSessionDto>($"Bird {bird.Name} is not ready for flight"));
        }

        // Check for existing active session
        existingSession = await _unitOfWork.FlightSessions.GetActiveByPlayerIdAsync(ownerId);
        if (existingSession != null)
            return BadRequest(Result.Failure<FlightSessionDto>("Player already has an active flight"));

        // Create flight session
        var session = FlightSession.Create(
            ownerId,
            request.BirdIds,
            request.Latitude,
            request.Longitude,
            TimeSpan.FromMinutes(request.DurationMinutes)
        );

        // Update bird states
        foreach (var bird in birds)
        {
            bird.StartFlying();
            _unitOfWork.Birds.Update(bird);
        }

        await _unitOfWork.FlightSessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var dto = MapToDto(session, birds);

        // Notify via SignalR
        await _hubContext.Clients.Group($"player_{ownerId}").OnFlightStarted(dto);

        return Ok(Result.Success(dto));
    }

    [HttpPost("{sessionId}/end")]
    public async Task<ActionResult<Result>> EndFlight(Guid sessionId)
    {
        var session = await _unitOfWork.FlightSessions.GetByIdAsync(sessionId);
        if (session == null)
            return NotFound(Result.Failure("Flight session not found"));

        if (!session.IsActive)
            return BadRequest(Result.Failure("Flight session is already ended"));

        session.End();
        _unitOfWork.FlightSessions.Update(session);

        // Return birds to coop
        var birds = await _unitOfWork.Birds.GetByIdsAsync(session.BirdIds);
        foreach (var bird in birds)
        {
            bird.ReturnToCoop();
            bird.ConsumeStamina(20);
            _unitOfWork.Birds.Update(bird);
        }

        await _unitOfWork.SaveChangesAsync();

        // Notify via SignalR
        await _hubContext.Clients.Group($"player_{session.PlayerId}").OnFlightEnded(sessionId);

        return Ok(Result.Success());
    }

    [HttpPut("{sessionId}/position")]
    public async Task<ActionResult<Result>> UpdatePosition(Guid sessionId, [FromBody] FlightPositionUpdate update)
    {
        var session = await _unitOfWork.FlightSessions.GetByIdAsync(sessionId);
        if (session == null)
            return NotFound(Result.Failure("Flight session not found"));

        if (!session.IsActive)
            return BadRequest(Result.Failure("Flight session is not active"));

        session.UpdatePosition(update.Latitude, update.Longitude, update.Altitude);
        _unitOfWork.FlightSessions.Update(session);
        await _unitOfWork.SaveChangesAsync();

        // Broadcast position to active flights
        await _hubContext.Clients.Group("active_flights").OnPositionUpdated(update);

        return Ok(Result.Success());
    }

    [HttpGet("active")]
    public async Task<ActionResult<Result<IEnumerable<FlightSessionDto>>>> GetActiveFlights()
    {
        var sessions = await _unitOfWork.FlightSessions.GetAllActiveAsync();
        var result = new List<FlightSessionDto>();

        foreach (var session in sessions)
        {
            var birds = await _unitOfWork.Birds.GetByIdsAsync(session.BirdIds);
            result.Add(MapToDto(session, birds));
        }

        return Ok(Result.Success<IEnumerable<FlightSessionDto>>(result));
    }

    private static FlightSessionDto MapToDto(FlightSession session, IEnumerable<Bird> birds) => new()
    {
        Id = session.Id,
        PlayerId = session.PlayerId,
        Birds = birds.Select(b => new BirdSummaryDto
        {
            Id = b.Id,
            Name = b.Name,
            State = (Shared.Enums.BirdStateDto)b.State,
            Rarity = (Shared.Enums.BirdRarityDto)b.Rarity,
            Element = (Shared.Enums.ElementDto)b.DNA.Element,
            TotalPower = b.Stats.GetTotalPower()
        }).ToList(),
        Latitude = session.Latitude,
        Longitude = session.Longitude,
        StartedAt = session.StartedAt,
        EndsAt = session.StartedAt.Add(session.Duration),
        IsActive = session.IsActive,
        EncountersCount = session.EncountersCount
    };
}
