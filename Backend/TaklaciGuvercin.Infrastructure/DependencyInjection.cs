using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaklaciGuvercin.Application.Interfaces;
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBirdRepository, BirdRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IFlightSessionRepository, FlightSessionRepository>();
        services.AddScoped<IEncounterRepository, EncounterRepository>();

        services.AddSignalR();

        return services;
    }
}
