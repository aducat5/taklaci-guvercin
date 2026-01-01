using Microsoft.EntityFrameworkCore;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Infrastructure.Data;

namespace TaklaciGuvercin.Infrastructure.Repositories;

public class FlightSessionRepository : Repository<FlightSession>, IFlightSessionRepository
{
    public FlightSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<FlightSession?> GetActiveByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.PlayerId == playerId && f.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<FlightSession>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FlightSession>> GetActiveInAreaAsync(
        double latitude,
        double longitude,
        double radiusMeters,
        CancellationToken cancellationToken = default)
    {
        // Approximate degree to meter conversion at equator
        const double metersPerDegree = 111320;
        var latRange = radiusMeters / metersPerDegree;
        var lonRange = radiusMeters / (metersPerDegree * Math.Cos(latitude * Math.PI / 180));

        return await _dbSet
            .Where(f => f.IsActive &&
                        f.Latitude >= latitude - latRange &&
                        f.Latitude <= latitude + latRange &&
                        f.Longitude >= longitude - lonRange &&
                        f.Longitude <= longitude + lonRange)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FlightSession>> GetExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => t.Result.Where(f => f.IsExpired()), cancellationToken);
    }
}
