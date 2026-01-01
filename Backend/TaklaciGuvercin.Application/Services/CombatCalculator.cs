using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;

namespace TaklaciGuvercin.Application.Services;

public static class CombatCalculator
{
    private static readonly Random _random = new();

    private static readonly Dictionary<BirdRarity, double> RarityMultipliers = new()
    {
        { BirdRarity.Common, 1.0 },
        { BirdRarity.Uncommon, 1.15 },
        { BirdRarity.Rare, 1.35 },
        { BirdRarity.Epic, 1.6 },
        { BirdRarity.Legendary, 1.85 },
        { BirdRarity.Mythical, 2.0 }
    };

    public static int CalculateBirdPower(Bird bird)
    {
        // Base power from stats
        var basePower = bird.Stats.GetTotalPower();

        // Health multiplier (0.5 - 1.0)
        var healthMultiplier = 0.5 + (0.5 * bird.Health / bird.MaxHealth);

        // Stamina multiplier (0.7 - 1.0)
        var staminaMultiplier = 0.7 + (0.3 * bird.Stamina / bird.MaxStamina);

        // Rarity bonus
        var rarityBonus = RarityMultipliers.GetValueOrDefault(bird.Rarity, 1.0);

        return (int)(basePower * healthMultiplier * staminaMultiplier * rarityBonus);
    }

    public static int CalculateFlockPower(IEnumerable<Bird> birds)
    {
        var birdList = birds.ToList();
        if (!birdList.Any()) return 0;

        var totalPower = birdList.Sum(CalculateBirdPower);

        // Element advantage bonus (up to +10% per matching element)
        var elementBonus = CalculateElementSynergy(birdList);

        // Flock synergy (+5% per bird with same element)
        var flockSynergy = CalculateFlockSynergy(birdList);

        // Leadership bonus (up to +20% based on highest leadership)
        var leadershipBonus = CalculateLeadershipBonus(birdList);

        return (int)(totalPower * elementBonus * flockSynergy * leadershipBonus);
    }

    private static double CalculateElementSynergy(List<Bird> birds)
    {
        // Group birds by element and calculate synergy
        var elementGroups = birds.GroupBy(b => b.DNA.Element).Where(g => g.Key != Element.None);
        var maxGroupSize = elementGroups.Any() ? elementGroups.Max(g => g.Count()) : 0;

        // +5% for each bird in the largest element group
        return 1.0 + (0.05 * maxGroupSize);
    }

    private static double CalculateFlockSynergy(List<Bird> birds)
    {
        if (birds.Count <= 1) return 1.0;

        // +3% for each additional bird
        return 1.0 + (0.03 * (birds.Count - 1));
    }

    private static double CalculateLeadershipBonus(List<Bird> birds)
    {
        if (!birds.Any()) return 1.0;

        var highestLeadership = birds.Max(b => b.Stats.Leadership);

        // Up to +20% based on highest leadership
        return 1.0 + (0.2 * highestLeadership / 100.0);
    }

    public static double CalculateElementAdvantage(Element attacker, Element defender)
    {
        // Fire > Ice > Wind > Emerald > Fire
        if (attacker == Element.None || defender == Element.None)
            return 1.0;

        if ((attacker == Element.Fire && defender == Element.Ice) ||
            (attacker == Element.Ice && defender == Element.Wind) ||
            (attacker == Element.Wind && defender == Element.Emerald) ||
            (attacker == Element.Emerald && defender == Element.Fire))
        {
            return 1.15; // 15% advantage
        }

        if ((defender == Element.Fire && attacker == Element.Ice) ||
            (defender == Element.Ice && attacker == Element.Wind) ||
            (defender == Element.Wind && attacker == Element.Emerald) ||
            (defender == Element.Emerald && attacker == Element.Fire))
        {
            return 0.85; // 15% disadvantage
        }

        return 1.0;
    }

    public static (Guid WinnerId, int RandomRoll) DetermineWinner(
        Guid player1Id,
        int power1,
        Guid player2Id,
        int power2)
    {
        var totalPower = power1 + power2;
        if (totalPower == 0) totalPower = 1;

        // Win chance based on power ratio
        var player1WinChance = (double)power1 / totalPower * 100;

        var roll = _random.Next(0, 101);

        return roll <= player1WinChance
            ? (player1Id, roll)
            : (player2Id, roll);
    }

    public static int CalculateBirdsLost(int loserBirdCount, int winnerPower, int loserPower)
    {
        if (loserBirdCount <= 1) return 0; // Always keep at least 1 bird

        var powerRatio = (double)winnerPower / Math.Max(loserPower, 1);

        // Base 10% + up to 30% based on power differential
        var lossPercentage = 0.1 + Math.Min(0.3, (powerRatio - 1) * 0.15);

        var birdsToLose = (int)Math.Ceiling(loserBirdCount * lossPercentage);

        // Minimum 1 bird lost, but always keep at least 1
        return Math.Min(birdsToLose, loserBirdCount - 1);
    }

    public static int CalculateCoinsReward(int loserPower, int loserLevel, int winnerLevel)
    {
        const int baseReward = 50;

        var powerBonus = (int)(loserPower * 0.5);
        var levelBonus = Math.Max(0, (loserLevel - winnerLevel) * 25);

        return baseReward + powerBonus + levelBonus;
    }

    public static int CalculateExperienceReward(int loserPower, bool isWinner)
    {
        const int baseXp = 25;
        const int maxXp = 500;

        if (!isWinner)
        {
            return baseXp / 2; // Participation reward
        }

        var difficultyBonus = loserPower * 0.1;

        return Math.Min(maxXp, (int)(baseXp + difficultyBonus));
    }

    public static List<Bird> SelectBirdsToLose(IEnumerable<Bird> birds, int count)
    {
        var birdList = birds.OrderBy(b => b.Stats.Loyalty).ToList();

        // Birds with lowest loyalty are lost first
        return birdList.Take(count).ToList();
    }
}
