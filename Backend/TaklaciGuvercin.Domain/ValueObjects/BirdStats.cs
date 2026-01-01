namespace TaklaciGuvercin.Domain.ValueObjects;

public class BirdStats
{
    public int Leadership { get; private set; }
    public int Loyalty { get; private set; }
    public int Speed { get; private set; }
    public int GeneticDominance { get; private set; }

    public const int MinStatValue = 1;
    public const int MaxStatValue = 100;

    private BirdStats() { }

    public static BirdStats Create(int leadership, int loyalty, int speed, int geneticDominance)
    {
        return new BirdStats
        {
            Leadership = Clamp(leadership),
            Loyalty = Clamp(loyalty),
            Speed = Clamp(speed),
            GeneticDominance = Clamp(geneticDominance)
        };
    }

    public static BirdStats CreateRandom(Random? random = null)
    {
        var rng = random ?? Random.Shared;
        return new BirdStats
        {
            Leadership = rng.Next(MinStatValue, MaxStatValue + 1),
            Loyalty = rng.Next(MinStatValue, MaxStatValue + 1),
            Speed = rng.Next(MinStatValue, MaxStatValue + 1),
            GeneticDominance = rng.Next(MinStatValue, MaxStatValue + 1)
        };
    }

    public int GetTotalPower() => Leadership + Loyalty + Speed + GeneticDominance;

    private static int Clamp(int value) => Math.Clamp(value, MinStatValue, MaxStatValue);
}
