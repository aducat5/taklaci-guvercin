using Microsoft.Extensions.Logging;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Services;

public class FlightManagementService : IFlightManagementService
{
    private readonly IFlightSessionRepository _flightRepository;
    private readonly IBirdRepository _birdRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FlightManagementService> _logger;

    public FlightManagementService(
        IFlightSessionRepository flightRepository,
        IBirdRepository birdRepository,
        IUnitOfWork unitOfWork,
        ILogger<FlightManagementService> logger)
    {
        _flightRepository = flightRepository;
        _birdRepository = birdRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FlightSession?> GetActiveFlightByPlayerIdAsync(Guid playerId)
    {
        var flights = await _flightRepository.GetActiveFlightsAsync();
        return flights.FirstOrDefault(f => f.PlayerId == playerId);
    }

    public async Task<IEnumerable<FlightSession>> GetFlightHistoryAsync(Guid playerId, int count = 10)
    {
        return await _flightRepository.GetByPlayerIdAsync(playerId, count);
    }

    public async Task<IEnumerable<FlightSession>> GetNearbyFlightsAsync(Guid sessionId, double radiusMeters = 500)
    {
        var session = await _flightRepository.GetByIdAsync(sessionId);
        if (session == null || !session.IsActive)
            return Enumerable.Empty<FlightSession>();

        var allActive = await _flightRepository.GetActiveFlightsAsync();

        return allActive.Where(f =>
            f.Id != sessionId &&
            f.IsActive &&
            session.IsInEncounterRange(f, radiusMeters));
    }

    public async Task<IEnumerable<FlightSession>> GetAllActiveFlightsAsync()
    {
        return await _flightRepository.GetActiveFlightsAsync();
    }

    public async Task<int> ExpireCompletedFlightsAsync()
    {
        var activeFlights = await _flightRepository.GetActiveFlightsAsync();
        var now = DateTime.UtcNow;
        var expiredCount = 0;

        foreach (var flight in activeFlights)
        {
            var endTime = flight.StartedAt + flight.Duration;
            if (now >= endTime)
            {
                await EndFlightInternalAsync(flight);
                expiredCount++;
            }
        }

        if (expiredCount > 0)
        {
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} flights", expiredCount);
        }

        return expiredCount;
    }

    public async Task<FlightSession?> EndFlightAsync(Guid sessionId)
    {
        var session = await _flightRepository.GetByIdAsync(sessionId);
        if (session == null || !session.IsActive)
            return null;

        await EndFlightInternalAsync(session);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Flight {SessionId} ended manually", sessionId);
        return session;
    }

    public async Task UpdateFlightPositionAsync(Guid sessionId, double latitude, double longitude, double altitude)
    {
        var session = await _flightRepository.GetByIdAsync(sessionId);
        if (session == null || !session.IsActive)
            return;

        session.UpdatePosition(latitude, longitude, altitude);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EndFlightInternalAsync(FlightSession flight)
    {
        flight.End();

        // Return birds to coop and consume stamina
        foreach (var birdId in flight.BirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null)
            {
                bird.ReturnToCoop();
                bird.ConsumeStamina(20);
            }
        }
    }
}
