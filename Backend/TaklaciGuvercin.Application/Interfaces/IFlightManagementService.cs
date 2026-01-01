using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IFlightManagementService
{
    Task<FlightSession?> GetActiveFlightByPlayerIdAsync(Guid playerId);
    Task<IEnumerable<FlightSession>> GetFlightHistoryAsync(Guid playerId, int count = 10);
    Task<IEnumerable<FlightSession>> GetNearbyFlightsAsync(Guid sessionId, double radiusMeters = 500);
    Task<IEnumerable<FlightSession>> GetAllActiveFlightsAsync();
    Task<int> ExpireCompletedFlightsAsync();
    Task<FlightSession?> EndFlightAsync(Guid sessionId);
    Task UpdateFlightPositionAsync(Guid sessionId, double latitude, double longitude, double altitude);
}
