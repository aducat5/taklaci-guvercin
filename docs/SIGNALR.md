# Taklaci Guvercin - SignalR Reference

## Hub Endpoint
```
WebSocket: wss://localhost:5001/hubs/airspace
```

## Connection Flow

```
1. Connect to Hub
2. Call RegisterPlayer(playerId)
3. Start Flight via REST API
4. Call JoinAirspace(sessionId)
5. Send/Receive position updates
6. Handle encounter notifications
7. Call LeaveAirspace(sessionId) on flight end
```

---

## Client → Server Methods

### RegisterPlayer
Register the connection with a player ID.
```csharp
await hubConnection.InvokeAsync("RegisterPlayer", playerId);
```

### JoinAirspace
Join a flight session's notification group.
```csharp
await hubConnection.InvokeAsync("JoinAirspace", sessionId);
```

### LeaveAirspace
Leave a flight session's notification group.
```csharp
await hubConnection.InvokeAsync("LeaveAirspace", sessionId);
```

### UpdatePosition
Broadcast current position to all active flights.
```csharp
await hubConnection.InvokeAsync("UpdatePosition", new FlightPositionUpdate
{
    SessionId = sessionId,
    Latitude = 41.0082,
    Longitude = 28.9784,
    Altitude = 100
});
```

---

## Server → Client Events

### OnFlightStarted
Triggered when a flight session starts.
```csharp
hubConnection.On<FlightSessionDto>("OnFlightStarted", session =>
{
    Console.WriteLine($"Flight started: {session.Id}");
});
```

**Payload:**
```json
{
  "id": "guid",
  "playerId": "guid",
  "birds": [...],
  "latitude": 41.0082,
  "longitude": 28.9784,
  "startedAt": "2024-01-01T12:00:00Z",
  "endsAt": "2024-01-01T12:30:00Z",
  "isActive": true,
  "encountersCount": 0
}
```

### OnFlightEnded
Triggered when a flight session ends.
```csharp
hubConnection.On<Guid>("OnFlightEnded", sessionId =>
{
    Console.WriteLine($"Flight ended: {sessionId}");
});
```

### OnPositionUpdated
Triggered when any active flight updates position.
```csharp
hubConnection.On<FlightPositionUpdate>("OnPositionUpdated", update =>
{
    Console.WriteLine($"Position: {update.Latitude}, {update.Longitude}");
});
```

**Payload:**
```json
{
  "sessionId": "guid",
  "latitude": 41.0082,
  "longitude": 28.9784,
  "altitude": 100
}
```

### OnEncounterDetected
Triggered when your flight encounters another.
```csharp
hubConnection.On<EncounterNotification>("OnEncounterDetected", notif =>
{
    Console.WriteLine($"Encounter! Opponent: {notif.OpponentPlayer.Username}");
    Console.WriteLine($"Your Power: {notif.YourTotalPower}");
    Console.WriteLine($"Their Power: {notif.OpponentTotalPower}");
});
```

**Payload:**
```json
{
  "encounterId": "guid",
  "opponentPlayer": {
    "id": "guid",
    "username": "Opponent",
    "level": 5,
    "isOnline": true
  },
  "opponentBirds": [...],
  "opponentTotalPower": 450,
  "yourTotalPower": 520,
  "timeToRespondSeconds": 30
}
```

### OnEncounterResult
Triggered when an encounter is resolved.
```csharp
hubConnection.On<EncounterResultNotification>("OnEncounterResult", result =>
{
    if (result.YouWon)
    {
        Console.WriteLine($"Victory! Gained {result.BirdsGained.Count} birds");
        Console.WriteLine($"Coins: +{result.CoinsChange}");
    }
    else
    {
        Console.WriteLine($"Defeat! Lost {result.BirdsLost.Count} birds");
    }
    Console.WriteLine($"XP: +{result.ExperienceGained}");
});
```

**Payload:**
```json
{
  "encounterId": "guid",
  "youWon": true,
  "birdsLost": [],
  "birdsGained": [...],
  "coinsChange": 150,
  "experienceGained": 75
}
```

### OnBirdStatusChanged
Triggered when a bird's status changes.
```csharp
hubConnection.On<BirdSummaryDto>("OnBirdStatusChanged", bird =>
{
    Console.WriteLine($"Bird {bird.Name} status: {bird.State}");
});
```

### OnError
Triggered on errors.
```csharp
hubConnection.On<string>("OnError", message =>
{
    Console.WriteLine($"Error: {message}");
});
```

---

## Groups

| Group Name | Purpose | Join When |
|------------|---------|-----------|
| `player_{playerId}` | Player-specific notifications | On RegisterPlayer |
| `airspace_{sessionId}` | Flight-specific notifications | On JoinAirspace |
| `active_flights` | All active flights | On JoinAirspace |

---

## Unity C# Example

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class AirspaceConnection : MonoBehaviour
{
    private HubConnection _connection;

    async void Start()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/hubs/airspace")
            .WithAutomaticReconnect()
            .Build();

        // Register handlers
        _connection.On<EncounterNotification>("OnEncounterDetected", OnEncounter);
        _connection.On<EncounterResultNotification>("OnEncounterResult", OnResult);

        await _connection.StartAsync();
        await _connection.InvokeAsync("RegisterPlayer", GameManager.PlayerId);
    }

    void OnEncounter(EncounterNotification notif)
    {
        // Show encounter UI
        UIManager.ShowEncounterAlert(notif);
    }

    void OnResult(EncounterResultNotification result)
    {
        // Show result UI
        UIManager.ShowEncounterResult(result);
    }

    async void OnDestroy()
    {
        await _connection?.DisposeAsync();
    }
}
```

---

## Connection States

| State | Description |
|-------|-------------|
| Connecting | Initial connection attempt |
| Connected | Active connection |
| Reconnecting | Lost connection, attempting reconnect |
| Disconnected | No connection |

Handle reconnection:
```csharp
_connection.Reconnecting += error =>
{
    Debug.Log("Reconnecting...");
    return Task.CompletedTask;
};

_connection.Reconnected += connectionId =>
{
    Debug.Log($"Reconnected: {connectionId}");
    // Re-register player
    return _connection.InvokeAsync("RegisterPlayer", GameManager.PlayerId);
};
```
