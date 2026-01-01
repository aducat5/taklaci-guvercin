using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Domain.ValueObjects;

namespace TaklaciGuvercin.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeeder> _logger;
    private static readonly Random _random = new();

    public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully");

            if (!await context.Players.AnyAsync())
            {
                await SeedPlayersAndBirdsAsync(context);
                _logger.LogInformation("Test data seeded successfully");
            }
            else
            {
                _logger.LogInformation("Database already contains data, skipping seed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedPlayersAndBirdsAsync(ApplicationDbContext context)
    {
        var players = new List<Player>
        {
            Player.Create("TestPlayer1", "player1@test.com", "hashedpassword123"),
            Player.Create("TestPlayer2", "player2@test.com", "hashedpassword123"),
            Player.Create("TestPlayer3", "player3@test.com", "hashedpassword123"),
        };

        await context.Players.AddRangeAsync(players);
        await context.SaveChangesAsync();

        foreach (var player in players)
        {
            var birds = CreateStarterBirds(player.Id, 5);
            await context.Birds.AddRangeAsync(birds);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("Created {PlayerCount} players with {BirdCount} birds each",
            players.Count, 5);
    }

    private List<Bird> CreateStarterBirds(Guid ownerId, int count)
    {
        var birds = new List<Bird>();
        var names = new[] { "Kanat", "Bulut", "Yildirim", "Ruzgar", "Golge", "Ates", "Buz", "Firtina" };

        for (int i = 0; i < count; i++)
        {
            var name = $"{names[_random.Next(names.Length)]}-{_random.Next(1000, 9999)}";
            var rarity = GetRandomRarity();
            var stats = BirdStats.CreateRandom(_random);
            var dna = CreateRandomDNA();

            var bird = Bird.Create(name, ownerId, dna, stats, rarity);
            birds.Add(bird);
        }

        return birds;
    }

    private BirdRarity GetRandomRarity()
    {
        var roll = _random.Next(100);
        return roll switch
        {
            < 50 => BirdRarity.Common,
            < 75 => BirdRarity.Uncommon,
            < 90 => BirdRarity.Rare,
            < 97 => BirdRarity.Epic,
            < 99 => BirdRarity.Legendary,
            _ => BirdRarity.Mythical
        };
    }

    private BirdDNA CreateRandomDNA()
    {
        var elements = Enum.GetValues<Element>();
        var element = elements[_random.Next(elements.Length)];

        return BirdDNA.Create(
            primaryColor: CreateRandomGene("PrimaryColor", new[] { "White", "Gray", "Black", "Brown", "Blue", "Red" }),
            secondaryColor: CreateRandomGene("SecondaryColor", new[] { "None", "White", "Black", "Gold", "Silver" }),
            pattern: CreateRandomGene("Pattern", new[] { "Solid", "Spotted", "Striped", "Mottled", "Barred" }),
            tailType: CreateRandomGene("TailType", new[] { "Fan", "Standard", "Forked", "Long", "Short" }),
            crestType: CreateRandomGene("CrestType", new[] { "None", "Peaked", "Shell", "Hooded", "Crown" }),
            elementGene: CreateRandomGene("Element", new[] { "Fire", "Ice", "Wind", "Emerald", "None" }),
            element: element,
            hiddenTrait1: CreateRandomGene("Hidden1", new[] { "A", "B", "C", "D" }),
            hiddenTrait2: CreateRandomGene("Hidden2", new[] { "X", "Y", "Z", "W" }),
            mutationFactor: (float)(_random.NextDouble() * 0.3 + 0.05)
        );
    }

    private Gene CreateRandomGene(string traitName, string[] alleles)
    {
        var geneTypes = Enum.GetValues<GeneType>();
        return Gene.Create(
            traitName,
            alleles[_random.Next(alleles.Length)],
            alleles[_random.Next(alleles.Length)],
            geneTypes[_random.Next(geneTypes.Length)]
        );
    }
}
