namespace TaklaciGuvercin.Shared.Enums
{
    public enum BirdState
    {
        InCoop = 0,
        Flying = 1,
        Sick = 2,
        Resting = 3
    }

    public enum BirdRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythical = 5
    }

    public enum Element
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Wind = 3,
        Emerald = 4
    }

    public enum EncounterState
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
        Cancelled = 3
    }
}
