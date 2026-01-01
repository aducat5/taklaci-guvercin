using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaklaciGuvercin.Application.Interfaces;
using TaklaciGuvercin.Application.Services;
using TaklaciGuvercin.Infrastructure.BackgroundServices;
using TaklaciGuvercin.Infrastructure.Data;
using TaklaciGuvercin.Infrastructure.Repositories;

namespace TaklaciGuvercin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
            }));

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBirdRepository, BirdRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IFlightSessionRepository, FlightSessionRepository>();
        services.AddScoped<IEncounterRepository, EncounterRepository>();

        // Application Services
        services.AddScoped<IFlightManagementService, FlightManagementService>();
        services.AddScoped<IEncounterService, EncounterService>();

        // Background Services
        services.AddHostedService<FlightExpirationService>();
        services.AddHostedService<EncounterDetectionService>();

        // Database Seeder
        services.AddScoped<DatabaseSeeder>();

        services.AddSignalR();

        return services;
    }
}
