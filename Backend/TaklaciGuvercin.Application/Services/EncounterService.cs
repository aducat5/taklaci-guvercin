using Microsoft.Extensions.Logging;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;

namespace TaklaciGuvercin.Application.Services;

public class EncounterService : IEncounterService
{
    private readonly IEncounterRepository _encounterRepository;
    private readonly IFlightSessionRepository _flightRepository;
    private readonly IBirdRepository _birdRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EncounterService> _logger;

    private const int EncounterTimeoutSeconds = 30;

    public EncounterService(
        IEncounterRepository encounterRepository,
        IFlightSessionRepository flightRepository,
        IBirdRepository birdRepository,
        IPlayerRepository playerRepository,
        IUnitOfWork unitOfWork,
        ILogger<EncounterService> logger)
    {
        _encounterRepository = encounterRepository;
        _flightRepository = flightRepository;
        _birdRepository = birdRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Encounter?> GetByIdAsync(Guid encounterId)
    {
        return await _encounterRepository.GetByIdAsync(encounterId);
    }

    public async Task<IEnumerable<Encounter>> GetActiveEncountersForPlayerAsync(Guid playerId)
    {
        return await _encounterRepository.GetActiveForPlayerAsync(playerId);
    }

    public async Task<IEnumerable<Encounter>> GetEncounterHistoryAsync(Guid playerId, int count = 10)
    {
        return await _encounterRepository.GetByPlayerIdAsync(playerId, count);
    }

    public async Task<Encounter> CreateEncounterAsync(FlightSession initiator, FlightSession target)
    {
        var initiatorPlayer = await _playerRepository.GetByIdAsync(initiator.PlayerId);
        var targetPlayer = await _playerRepository.GetByIdAsync(target.PlayerId);

        var encounter = Encounter.Create(
            initiator.Id,
            target.Id,
            initiator.PlayerId,
            target.PlayerId,
            initiatorPlayer?.IsOnline ?? false,
            targetPlayer?.IsOnline ?? false);

        await _encounterRepository.AddAsync(encounter);

        // Record encounter on flight sessions
        initiator.RecordEncounter();
        target.RecordEncounter();

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Created encounter {EncounterId} between {InitiatorId} and {TargetId}",
            encounter.Id, initiator.PlayerId, target.PlayerId);

        return encounter;
    }

    public async Task<Encounter?> ResolveEncounterAsync(Guid encounterId)
    {
        var encounter = await _encounterRepository.GetByIdAsync(encounterId);
        if (encounter == null || encounter.State == EncounterState.Resolved)
            return encounter;

        encounter.SetInProgress();

        // Get birds for each side
        var initiatorSession = await _flightRepository.GetByIdAsync(encounter.InitiatorSessionId);
        var targetSession = await _flightRepository.GetByIdAsync(encounter.TargetSessionId);

        if (initiatorSession == null || targetSession == null)
        {
            encounter.Cancel();
            await _unitOfWork.SaveChangesAsync();
            return encounter;
        }

        var initiatorBirds = new List<Bird>();
        var targetBirds = new List<Bird>();

        foreach (var birdId in initiatorSession.BirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null) initiatorBirds.Add(bird);
        }

        foreach (var birdId in targetSession.BirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null) targetBirds.Add(bird);
        }

        // Calculate powers
        var initiatorPower = CombatCalculator.CalculateFlockPower(initiatorBirds);
        var targetPower = CombatCalculator.CalculateFlockPower(targetBirds);

        // Determine winner
        var (winnerId, roll) = CombatCalculator.DetermineWinner(
            encounter.InitiatorPlayerId, initiatorPower,
            encounter.TargetPlayerId, targetPower);

        encounter.SetCombatPowers(initiatorPower, targetPower, roll);

        // Process results
        var isInitiatorWinner = winnerId == encounter.InitiatorPlayerId;
        var winnerPlayer = await _playerRepository.GetByIdAsync(winnerId);
        var loserPlayer = await _playerRepository.GetByIdAsync(
            isInitiatorWinner ? encounter.TargetPlayerId : encounter.InitiatorPlayerId);

        var loserBirds = isInitiatorWinner ? targetBirds : initiatorBirds;
        var winnerPower = isInitiatorWinner ? initiatorPower : targetPower;
        var loserPower = isInitiatorWinner ? targetPower : initiatorPower;

        // Calculate loot
        var birdsToLoseCount = CombatCalculator.CalculateBirdsLost(
            loserBirds.Count, winnerPower, loserPower);
        var lostBirds = CombatCalculator.SelectBirdsToLose(loserBirds, birdsToLoseCount);
        var lootedBirdIds = lostBirds.Select(b => b.Id).ToList();

        // Transfer birds
        foreach (var bird in lostBirds)
        {
            bird.TransferOwnership(winnerId);
        }

        // Calculate and transfer coins
        var coinsReward = CombatCalculator.CalculateCoinsReward(
            loserPower,
            loserPlayer?.Level ?? 1,
            winnerPlayer?.Level ?? 1);

        if (winnerPlayer != null) winnerPlayer.AddCoins(coinsReward);

        // Award experience
        var winnerXp = CombatCalculator.CalculateExperienceReward(loserPower, true);
        var loserXp = CombatCalculator.CalculateExperienceReward(0, false);

        if (winnerPlayer != null)
        {
            winnerPlayer.AddExperience(winnerXp);
            winnerPlayer.RecordEncounterWin();
            foreach (var _ in lostBirds) winnerPlayer.RecordBirdLooted();
        }

        if (loserPlayer != null)
        {
            loserPlayer.AddExperience(loserXp);
            loserPlayer.RecordEncounterLoss();
            foreach (var _ in lostBirds) loserPlayer.RecordBirdLost();
        }

        encounter.Resolve(winnerId, lootedBirdIds, coinsReward);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Resolved encounter {EncounterId}: Winner={WinnerId}, BirdsLost={BirdsLost}, Coins={Coins}",
            encounter.Id, winnerId, lootedBirdIds.Count, coinsReward);

        return encounter;
    }

    public async Task<Encounter?> CancelEncounterAsync(Guid encounterId)
    {
        var encounter = await _encounterRepository.GetByIdAsync(encounterId);
        if (encounter == null || encounter.State != EncounterState.Pending)
            return encounter;

        encounter.Cancel();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Cancelled encounter {EncounterId}", encounterId);
        return encounter;
    }

    public async Task<int> ResolveTimedOutEncountersAsync()
    {
        var pendingEncounters = await _encounterRepository.GetPendingAsync();
        var now = DateTime.UtcNow;
        var resolvedCount = 0;

        foreach (var encounter in pendingEncounters)
        {
            var timeout = encounter.CreatedAt.AddSeconds(EncounterTimeoutSeconds);
            if (now >= timeout)
            {
                await ResolveEncounterAsync(encounter.Id);
                resolvedCount++;
            }
        }

        if (resolvedCount > 0)
        {
            _logger.LogInformation("Auto-resolved {Count} timed-out encounters", resolvedCount);
        }

        return resolvedCount;
    }

    public async Task<EncounterPreview> PreviewEncounterAsync(FlightSession session1, FlightSession session2)
    {
        var session1Birds = new List<Bird>();
        var session2Birds = new List<Bird>();

        foreach (var birdId in session1.BirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null) session1Birds.Add(bird);
        }

        foreach (var birdId in session2.BirdIds)
        {
            var bird = await _birdRepository.GetByIdAsync(birdId);
            if (bird != null) session2Birds.Add(bird);
        }

        var power1 = CombatCalculator.CalculateFlockPower(session1Birds);
        var power2 = CombatCalculator.CalculateFlockPower(session2Birds);

        var totalPower = power1 + power2;
        var winChance1 = totalPower > 0 ? (double)power1 / totalPower : 0.5;
        var winChance2 = totalPower > 0 ? (double)power2 / totalPower : 0.5;

        return new EncounterPreview(
            session1.Id,
            session2.Id,
            power1,
            power2,
            winChance1,
            winChance2,
            session1Birds,
            session2Birds);
    }
}
