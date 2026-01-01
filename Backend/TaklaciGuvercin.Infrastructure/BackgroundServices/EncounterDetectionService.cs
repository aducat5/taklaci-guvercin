using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Domain.Entities;
using TaklaciGuvercin.Infrastructure.Hubs;
using TaklaciGuvercin.Shared.DTOs;
using TaklaciGuvercin.Shared.Enums;

namespace TaklaciGuvercin.Infrastructure.BackgroundServices;

public class EncounterDetectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EncounterDetectionService> _logger;
    private readonly IHubContext<AirspaceHub, IAirspaceClient> _hubContext;
    private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(5);
    private const double EncounterRangeMeters = 500;

    public EncounterDetectionService(
        IServiceProvider serviceProvider,
        ILogger<EncounterDetectionService> logger,
        IHubContext<AirspaceHub, IAirspaceClient> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Encounter Detection Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForEncountersAsync(stoppingToken);
                await ResolveTimedOutEncountersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during encounter detection");
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }

        _logger.LogInformation("Encounter Detection Service stopped");
    }

    private async Task ScanForEncountersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var flightService = scope.ServiceProvider.GetRequiredService<IFlightManagementService>();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();
        var encounterRepository = scope.ServiceProvider.GetRequiredService<IEncounterRepository>();
        var birdRepository = scope.ServiceProvider.GetRequiredService<IBirdRepository>();

        var activeFlights = (await flightService.GetAllActiveFlightsAsync()).ToList();

        if (activeFlights.Count < 2) return;

        var checkedPairs = new HashSet<string>();

        foreach (var flight1 in activeFlights)
        {
            foreach (var flight2 in activeFlights)
            {
                if (flight1.Id == flight2.Id) continue;
                if (flight1.PlayerId == flight2.PlayerId) continue;

                // Create sorted pair key to avoid duplicate checks
                var pairKey = flight1.Id.CompareTo(flight2.Id) < 0
                    ? $"{flight1.Id}_{flight2.Id}"
                    : $"{flight2.Id}_{flight1.Id}";

                if (checkedPairs.Contains(pairKey)) continue;
                checkedPairs.Add(pairKey);

                if (!flight1.IsInEncounterRange(flight2, EncounterRangeMeters)) continue;

                // Check if there's already an active encounter between these sessions
                var existingEncounter = await encounterRepository.GetActiveEncounterBetweenSessionsAsync(
                    flight1.Id, flight2.Id, cancellationToken);

                if (existingEncounter != null) continue;

                // Create new encounter
                var encounter = await encounterService.CreateEncounterAsync(flight1, flight2);

                // Notify both players
                await NotifyEncounterDetectedAsync(encounter, flight1, flight2, birdRepository);

                _logger.LogInformation(
                    "Encounter detected between {Player1} and {Player2}",
                    flight1.PlayerId, flight2.PlayerId);
            }
        }
    }

    private async Task ResolveTimedOutEncountersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        await encounterService.ResolveTimedOutEncountersAsync();
    }

    private async Task NotifyEncounterDetectedAsync(
        Encounter encounter,
        FlightSession initiatorSession,
        FlightSession targetSession,
        IBirdRepository birdRepository)
    {
        var initiatorBirds = new List<BirdSummaryDto>();
        var targetBirds = new List<BirdSummaryDto>();

        foreach (var birdId in initiatorSession.BirdIds)
        {
            var bird = await birdRepository.GetByIdAsync(birdId);
            if (bird != null)
            {
                initiatorBirds.Add(MapToBirdSummary(bird));
            }
        }

        foreach (var birdId in targetSession.BirdIds)
        {
            var bird = await birdRepository.GetByIdAsync(birdId);
            if (bird != null)
            {
                targetBirds.Add(MapToBirdSummary(bird));
            }
        }

        var initiatorPower = initiatorBirds.Sum(b => b.TotalPower);
        var targetPower = targetBirds.Sum(b => b.TotalPower);

        // Notify initiator
        var initiatorNotification = new EncounterNotification
        {
            EncounterId = encounter.Id,
            OpponentPlayerId = targetSession.PlayerId,
            OpponentBirds = targetBirds,
            OpponentTotalPower = targetPower,
            YourTotalPower = initiatorPower,
            TimeToRespondSeconds = 30
        };

        // Notify target
        var targetNotification = new EncounterNotification
        {
            EncounterId = encounter.Id,
            OpponentPlayerId = initiatorSession.PlayerId,
            OpponentBirds = initiatorBirds,
            OpponentTotalPower = initiatorPower,
            YourTotalPower = targetPower,
            TimeToRespondSeconds = 30
        };

        await _hubContext.Clients.Group($"player_{initiatorSession.PlayerId}")
            .OnEncounterDetected(initiatorNotification);

        await _hubContext.Clients.Group($"player_{targetSession.PlayerId}")
            .OnEncounterDetected(targetNotification);
    }

    private static BirdSummaryDto MapToBirdSummary(Bird bird)
    {
        return new BirdSummaryDto
        {
            Id = bird.Id,
            Name = bird.Name,
            Rarity = (BirdRarityDto)bird.Rarity,
            State = (BirdStateDto)bird.State,
            Element = (ElementDto)bird.DNA.Element,
            Leadership = bird.Stats.Leadership,
            Loyalty = bird.Stats.Loyalty,
            Speed = bird.Stats.Speed,
            GeneticDominance = bird.Stats.GeneticDominance,
            TotalPower = bird.Stats.GetTotalPower(),
            Health = bird.Health,
            Stamina = bird.Stamina
        };
    }
}
