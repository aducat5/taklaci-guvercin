using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IEncounterRepository : IRepository<Encounter>
{
    Task<IEnumerable<Encounter>> GetByPlayerIdAsync(Guid playerId, int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Encounter>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Encounter>> GetActiveForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<Encounter?> GetActiveEncounterBetweenSessionsAsync(Guid session1Id, Guid session2Id, CancellationToken cancellationToken = default);
}
