using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IPlayerRepository : IRepository<Player>
{
    Task<Player?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Player?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<Player>> GetOnlinePlayersAsync(CancellationToken cancellationToken = default);
}
