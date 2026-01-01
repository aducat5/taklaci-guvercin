using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IBirdRepository : IRepository<Bird>
{
    Task<IEnumerable<Bird>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bird>> GetByOwnerIdAndStateAsync(Guid ownerId, BirdState state, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bird>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<Bird>> GetLineageAsync(Guid birdId, int generations = 3, CancellationToken cancellationToken = default);
    Task<int> GetOwnerBirdCountAsync(Guid ownerId, CancellationToken cancellationToken = default);
}
