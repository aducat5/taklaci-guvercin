using Microsoft.EntityFrameworkCore;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Infrastructure.Data;

namespace TaklaciGuvercin.Infrastructure.Repositories;

public class BirdRepository : Repository<Bird>, IBirdRepository
{
    public BirdRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Bird>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.OwnerId == ownerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bird>> GetByOwnerIdAndStateAsync(Guid ownerId, BirdState state, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.OwnerId == ownerId && b.State == state)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bird>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbSet
            .Where(b => idList.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Bird>> GetLineageAsync(Guid birdId, int generations = 3, CancellationToken cancellationToken = default)
    {
        var result = new List<Bird>();
        var currentGeneration = new List<Guid> { birdId };

        for (int i = 0; i < generations && currentGeneration.Any(); i++)
        {
            var birds = await _dbSet
                .Where(b => currentGeneration.Contains(b.Id))
                .ToListAsync(cancellationToken);

            result.AddRange(birds);

            currentGeneration = birds
                .SelectMany(b => new[] { b.MotherId, b.FatherId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
        }

        return result;
    }

    public async Task<int> GetOwnerBirdCountAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(b => b.OwnerId == ownerId, cancellationToken);
    }
}
