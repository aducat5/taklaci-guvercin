using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Infrastructure.Hubs;

namespace TaklaciGuvercin.Infrastructure.BackgroundServices;

public class FlightExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FlightExpirationService> _logger;
    private readonly IHubContext<AirspaceHub, IAirspaceClient> _hubContext;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public FlightExpirationService(
        IServiceProvider serviceProvider,
        ILogger<FlightExpirationService> logger,
        IHubContext<AirspaceHub, IAirspaceClient> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Flight Expiration Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExpireFlightsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking for expired flights");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Flight Expiration Service stopped");
    }

    private async Task CheckAndExpireFlightsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var flightService = scope.ServiceProvider.GetRequiredService<IFlightManagementService>();

        var expiredCount = await flightService.ExpireCompletedFlightsAsync();

        if (expiredCount > 0)
        {
            _logger.LogInformation("Expired {Count} flights", expiredCount);
        }
    }
}
