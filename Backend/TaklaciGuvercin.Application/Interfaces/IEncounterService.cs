using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Interfaces;

public interface IEncounterService
{
    Task<Encounter?> GetByIdAsync(Guid encounterId);
    Task<IEnumerable<Encounter>> GetActiveEncountersForPlayerAsync(Guid playerId);
    Task<IEnumerable<Encounter>> GetEncounterHistoryAsync(Guid playerId, int count = 10);
    Task<Encounter> CreateEncounterAsync(FlightSession initiator, FlightSession target);
    Task<Encounter?> ResolveEncounterAsync(Guid encounterId);
    Task<Encounter?> CancelEncounterAsync(Guid encounterId);
    Task<int> ResolveTimedOutEncountersAsync();
    Task<EncounterPreview> PreviewEncounterAsync(FlightSession session1, FlightSession session2);
}

public record EncounterPreview(
    Guid Session1Id,
    Guid Session2Id,
    int Power1,
    int Power2,
    double WinChance1,
    double WinChance2,
    List<Bird> Session1Birds,
    List<Bird> Session2Birds);
