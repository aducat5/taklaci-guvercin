namespace TaklaciGuvercin.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBirdRepository Birds { get; }
    IPlayerRepository Players { get; }
    IFlightSessionRepository FlightSessions { get; }
    IEncounterRepository Encounters { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
