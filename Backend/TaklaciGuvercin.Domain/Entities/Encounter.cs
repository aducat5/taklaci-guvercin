using TaklaciGuvercin.Domain.Common;

namespace TaklaciGuvercin.Domain.Entities;

public class Encounter : BaseEntity
{
    public Guid InitiatorSessionId { get; private set; }
    public Guid TargetSessionId { get; private set; }
    public Guid InitiatorPlayerId { get; private set; }
    public Guid TargetPlayerId { get; private set; }

    // Combat state
    public EncounterState State { get; private set; }
    public Guid? WinnerPlayerId { get; private set; }

    // Results
    public List<Guid> LootedBirdIds { get; private set; } = new();
    public int CoinsLooted { get; private set; }

    // Combat details
    public int InitiatorPower { get; private set; }
    public int TargetPower { get; private set; }
    public int RandomRoll { get; private set; }

    public DateTime? ResolvedAt { get; private set; }
    public bool InitiatorWasOnline { get; private set; }
    public bool TargetWasOnline { get; private set; }

    private Encounter() { }

    public static Encounter Create(
        Guid initiatorSessionId,
        Guid targetSessionId,
        Guid initiatorPlayerId,
        Guid targetPlayerId,
        bool initiatorOnline,
        bool targetOnline)
    {
        return new Encounter
        {
            InitiatorSessionId = initiatorSessionId,
            TargetSessionId = targetSessionId,
            InitiatorPlayerId = initiatorPlayerId,
            TargetPlayerId = targetPlayerId,
            State = EncounterState.Pending,
            InitiatorWasOnline = initiatorOnline,
            TargetWasOnline = targetOnline
        };
    }

    public void SetCombatPowers(int initiatorPower, int targetPower, int randomRoll)
    {
        InitiatorPower = initiatorPower;
        TargetPower = targetPower;
        RandomRoll = randomRoll;
        SetUpdated();
    }

    public void Resolve(Guid winnerPlayerId, List<Guid> lootedBirds, int coinsLooted)
    {
        State = EncounterState.Resolved;
        WinnerPlayerId = winnerPlayerId;
        LootedBirdIds = lootedBirds;
        CoinsLooted = coinsLooted;
        ResolvedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void SetInProgress()
    {
        State = EncounterState.InProgress;
        SetUpdated();
    }

    public void Cancel()
    {
        State = EncounterState.Cancelled;
        ResolvedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public bool IsInitiatorWinner() => WinnerPlayerId == InitiatorPlayerId;
    public bool IsTargetWinner() => WinnerPlayerId == TargetPlayerId;
}

public enum EncounterState
{
    Pending = 0,
    InProgress = 1,
    Resolved = 2,
    Cancelled = 3
}
