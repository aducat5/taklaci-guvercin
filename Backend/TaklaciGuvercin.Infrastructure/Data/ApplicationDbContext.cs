using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Domain.Enums;
using TaklaciGuvercin.Domain.ValueObjects;

namespace TaklaciGuvercin.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Bird> Birds => Set<Bird>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<FlightSession> FlightSessions => Set<FlightSession>();
    public DbSet<Encounter> Encounters => Set<Encounter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBird(modelBuilder);
        ConfigurePlayer(modelBuilder);
        ConfigureFlightSession(modelBuilder);
        ConfigureEncounter(modelBuilder);
    }

    private static void ConfigureBird(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bird>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.State).HasConversion<string>();
            entity.Property(e => e.Rarity).HasConversion<string>();

            entity.OwnsOne(e => e.Stats, stats =>
            {
                stats.Property(s => s.Leadership).HasColumnName("Leadership");
                stats.Property(s => s.Loyalty).HasColumnName("Loyalty");
                stats.Property(s => s.Speed).HasColumnName("Speed");
                stats.Property(s => s.GeneticDominance).HasColumnName("GeneticDominance");
            });

            entity.OwnsOne(e => e.DNA, dna =>
            {
                dna.Property(d => d.Element).HasConversion<string>().HasColumnName("Element");
                dna.Property(d => d.MutationFactor).HasColumnName("MutationFactor");

                dna.OwnsOne(d => d.PrimaryColor, g => ConfigureGene(g, "PrimaryColor"));
                dna.OwnsOne(d => d.SecondaryColor, g => ConfigureGene(g, "SecondaryColor"));
                dna.OwnsOne(d => d.Pattern, g => ConfigureGene(g, "Pattern"));
                dna.OwnsOne(d => d.TailType, g => ConfigureGene(g, "TailType"));
                dna.OwnsOne(d => d.CrestType, g => ConfigureGene(g, "CrestType"));
                dna.OwnsOne(d => d.ElementGene, g => ConfigureGene(g, "ElementGene"));
                dna.OwnsOne(d => d.HiddenTrait1, g => ConfigureGene(g, "HiddenTrait1"));
                dna.OwnsOne(d => d.HiddenTrait2, g => ConfigureGene(g, "HiddenTrait2"));
            });

            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.State);
        });
    }

    private static void ConfigureGene<T>(OwnedNavigationBuilder<T, Gene> builder, string prefix) where T : class
    {
        builder.Property(g => g.TraitName).HasColumnName($"{prefix}_TraitName").HasMaxLength(50);
        builder.Property(g => g.Allele1).HasColumnName($"{prefix}_Allele1").HasMaxLength(50);
        builder.Property(g => g.Allele2).HasColumnName($"{prefix}_Allele2").HasMaxLength(50);
        builder.Property(g => g.Type).HasColumnName($"{prefix}_Type").HasConversion<string>();
    }

    private static void ConfigurePlayer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(512).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }

    private static void ConfigureFlightSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BirdIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList()
                );

            entity.HasIndex(e => e.PlayerId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });
    }

    private static void ConfigureEncounter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.State).HasConversion<string>();
            entity.Property(e => e.LootedBirdIds)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList()
                );

            entity.HasIndex(e => e.InitiatorPlayerId);
            entity.HasIndex(e => e.TargetPlayerId);
            entity.HasIndex(e => e.State);
        });
    }
}
