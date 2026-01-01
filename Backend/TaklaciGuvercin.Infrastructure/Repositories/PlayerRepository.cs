using Microsoft.EntityFrameworkCore;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Infrastructure.Data;

namespace TaklaciGuvercin.Infrastructure.Repositories;

public class PlayerRepository : Repository<Player>, IPlayerRepository
{
    public PlayerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Player?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<Player?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower(), cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(p => p.Username.ToLower() == username.ToLower(), cancellationToken);
    }

    public async Task<IEnumerable<Player>> GetOnlinePlayersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsOnline)
            .ToListAsync(cancellationToken);
    }
}
