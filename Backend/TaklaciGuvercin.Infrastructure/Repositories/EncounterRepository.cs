using Microsoft.EntityFrameworkCore;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Infrastructure.Data;

namespace TaklaciGuvercin.Infrastructure.Repositories;

public class EncounterRepository : Repository<Encounter>, IEncounterRepository
{
    public EncounterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Encounter>> GetByPlayerIdAsync(Guid playerId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.InitiatorPlayerId == playerId || e.TargetPlayerId == playerId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Encounter>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.State == EncounterState.Pending || e.State == EncounterState.InProgress)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Encounter>> GetActiveForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e =>
                (e.InitiatorPlayerId == playerId || e.TargetPlayerId == playerId) &&
                (e.State == EncounterState.Pending || e.State == EncounterState.InProgress))
            .ToListAsync(cancellationToken);
    }

    public async Task<Encounter?> GetActiveEncounterBetweenSessionsAsync(
        Guid session1Id,
        Guid session2Id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e =>
                (e.State == EncounterState.Pending || e.State == EncounterState.InProgress) &&
                ((e.InitiatorSessionId == session1Id && e.TargetSessionId == session2Id) ||
                 (e.InitiatorSessionId == session2Id && e.TargetSessionId == session1Id)),
                cancellationToken);
    }
}
