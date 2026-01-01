using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IFlightSessionRepository : IRepository<FlightSession>
{
    Task<FlightSession?> GetActiveByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FlightSession>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FlightSession>> GetActiveInAreaAsync(double latitude, double longitude, double radiusMeters, CancellationToken cancellationToken = default);
    Task<IEnumerable<FlightSession>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default);
}
