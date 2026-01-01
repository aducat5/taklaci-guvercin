using Microsoft.AspNetCore.SignalR;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Infrastructure.Hubs;

public interface IAirspaceClient
{
    Task OnFlightStarted(FlightSessionDto session);
    Task OnFlightEnded(Guid sessionId);
    Task OnPositionUpdated(FlightPositionUpdate update);
    Task OnEncounterDetected(EncounterNotification notification);
    Task OnEncounterResult(EncounterResultNotification result);
    Task OnBirdStatusChanged(BirdSummaryDto bird);
    Task OnError(string message);
}

public class AirspaceHub : Hub<IAirspaceClient>
{
    private static readonly Dictionary<string, Guid> _connectionToPlayer = new();
    private static readonly Dictionary<Guid, string> _playerToConnection = new();

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
        {
            _connectionToPlayer.Remove(Context.ConnectionId);
            _playerToConnection.Remove(playerId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"player_{playerId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterPlayer(Guid playerId)
    {
        _connectionToPlayer[Context.ConnectionId] = playerId;
        _playerToConnection[playerId] = Context.ConnectionId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"player_{playerId}");
    }

    public async Task JoinAirspace(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"airspace_{sessionId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, "active_flights");
    }

    public async Task LeaveAirspace(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"airspace_{sessionId}");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "active_flights");
    }

    public async Task UpdatePosition(FlightPositionUpdate update)
    {
        await Clients.Group("active_flights").OnPositionUpdated(update);
    }

    public static string? GetConnectionId(Guid playerId)
    {
        return _playerToConnection.TryGetValue(playerId, out var connectionId) ? connectionId : null;
    }

    public static bool IsPlayerOnline(Guid playerId)
    {
        return _playerToConnection.ContainsKey(playerId);
    }
}
