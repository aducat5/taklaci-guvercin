using TaklaciGuvercin.Domain.Common;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Domain.ValueObjects;

namespace TaklaciGuvercin.Domain.Entities;

public class Bird : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public BirdState State { get; private set; }
    public BirdRarity Rarity { get; private set; }

    // Genetics
    public BirdDNA DNA { get; private set; } = null!;
    public BirdStats Stats { get; private set; } = null!;

    // Lineage
    public Guid? MotherId { get; private set; }
    public Guid? FatherId { get; private set; }
    public int Generation { get; private set; }

    // Health & Status
    public int Health { get; private set; }
    public int MaxHealth { get; private set; }
    public int Stamina { get; private set; }
    public int MaxStamina { get; private set; }
    public DateTime? LastFedAt { get; private set; }
    public DateTime? SickUntil { get; private set; }
    public DateTime? RestingUntil { get; private set; }

    private Bird() { }

    public static Bird Create(
        string name,
        Guid ownerId,
        BirdDNA dna,
        BirdStats stats,
        BirdRarity rarity = BirdRarity.Common,
        Guid? motherId = null,
        Guid? fatherId = null,
        int generation = 1)
    {
        var bird = new Bird
        {
            Name = name,
            OwnerId = ownerId,
            DNA = dna,
            Stats = stats,
            Rarity = rarity,
            State = BirdState.InCoop,
            MotherId = motherId,
            FatherId = fatherId,
            Generation = generation,
            MaxHealth = 100,
            Health = 100,
            MaxStamina = 100,
            Stamina = 100
        };

        return bird;
    }

    public void SetState(BirdState newState)
    {
        State = newState;
        SetUpdated();
    }

    public void StartFlying()
    {
        if (State != BirdState.InCoop)
            throw new InvalidOperationException("Bird must be in coop to start flying");

        if (Stamina < 20)
            throw new InvalidOperationException("Bird needs at least 20 stamina to fly");

        State = BirdState.Flying;
        SetUpdated();
    }

    public void ReturnToCoop()
    {
        State = BirdState.InCoop;
        SetUpdated();
    }

    public void MakeSick(TimeSpan duration)
    {
        State = BirdState.Sick;
        SickUntil = DateTime.UtcNow.Add(duration);
        SetUpdated();
    }

    public void StartResting(TimeSpan duration)
    {
        State = BirdState.Resting;
        RestingUntil = DateTime.UtcNow.Add(duration);
        SetUpdated();
    }

    public void Feed()
    {
        LastFedAt = DateTime.UtcNow;
        Stamina = Math.Min(Stamina + 30, MaxStamina);
        Health = Math.Min(Health + 10, MaxHealth);
        SetUpdated();
    }

    public void TakeDamage(int damage)
    {
        Health = Math.Max(Health - damage, 0);
        SetUpdated();
    }

    public void ConsumeStamina(int amount)
    {
        Stamina = Math.Max(Stamina - amount, 0);
        SetUpdated();
    }

    public void TransferOwnership(Guid newOwnerId)
    {
        OwnerId = newOwnerId;
        SetUpdated();
    }

    public void Rename(string newName)
    {
        Name = newName;
        SetUpdated();
    }

    public bool CanBreed()
    {
        return State == BirdState.InCoop && Health > 50 && Stamina > 30;
    }

    public bool IsReadyForFlight()
    {
        return State == BirdState.InCoop && Stamina >= 20;
    }
}
