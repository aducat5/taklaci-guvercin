# Taklaci Guvercin - API Reference

Base URL: `https://localhost:5001/api`

## Authentication

### Register
```http
POST /players/register
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

### Login
```http
POST /players/login
Content-Type: application/json

{
  "email": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "value": {
    "success": true,
    "token": "base64_token",
    "refreshToken": "guid",
    "player": { ... }
  }
}
```

---

## Players

### Get Player
```http
GET /players/{id}
```

### Logout
```http
POST /players/{id}/logout
```

### Add Coins
```http
PUT /players/{id}/coins
Content-Type: application/json

100
```

---

## Birds

### Get Player's Birds
```http
GET /birds/player/{playerId}
```

### Get Bird Details
```http
GET /birds/{id}
```

### Get Bird Lineage
```http
GET /birds/{id}/lineage?generations=3
```

### Create Bird
```http
POST /birds
Content-Type: application/json

{
  "name": "string",
  "ownerId": "guid"
}
```

### Feed Bird
```http
PUT /birds/{id}/feed
```

### Rename Bird
```http
PUT /birds/{id}/rename
Content-Type: application/json

{
  "newName": "string"
}
```

---

## Breeding

### Preview Breeding
```http
POST /breeding/preview
Content-Type: application/json

{
  "motherId": "guid",
  "fatherId": "guid"
}
```

### Breed Birds
```http
POST /breeding
Content-Type: application/json

{
  "motherId": "guid",
  "fatherId": "guid"
}
```

---

## Flights

### Get Active Session
```http
GET /flights/player/{playerId}
```

### Start Flight
```http
POST /flights/start
Content-Type: application/json

{
  "birdIds": ["guid", "guid"],
  "latitude": 41.0082,
  "longitude": 28.9784,
  "durationMinutes": 30
}
```

### End Flight
```http
POST /flights/{sessionId}/end
```

### Update Position
```http
PUT /flights/{sessionId}/position
Content-Type: application/json

{
  "sessionId": "guid",
  "latitude": 41.0082,
  "longitude": 28.9784,
  "altitude": 100
}
```

### Get Active Flights
```http
GET /flights/active
```

### Get Flight History
```http
GET /flights/player/{playerId}/history?count=10
```

### Get Nearby Flights
```http
GET /flights/{sessionId}/nearby?radiusMeters=1000
```

---

## Encounters

### Get Encounter
```http
GET /encounters/{encounterId}
```

### Get Active Encounters
```http
GET /encounters/player/{playerId}/active
```

### Get Encounter History
```http
GET /encounters/player/{playerId}/history?count=10
```

### Get Player Stats
```http
GET /encounters/player/{playerId}/stats
```

### Resolve Encounter
```http
POST /encounters/{encounterId}/resolve
```

### Cancel Encounter
```http
POST /encounters/{encounterId}/cancel
```

### Preview Encounter
```http
POST /encounters/preview
Content-Type: application/json

{
  "session1Id": "guid",
  "session2Id": "guid"
}
```

---

## Response Format

All responses are wrapped in a `Result<T>` object:

```json
{
  "isSuccess": true,
  "isFailure": false,
  "error": "",
  "errors": [],
  "value": { ... }
}
```

### Error Response
```json
{
  "isSuccess": false,
  "isFailure": true,
  "error": "Error message",
  "errors": ["Error 1", "Error 2"],
  "value": null
}
```

---

## SignalR Hub

**Endpoint:** `/hubs/airspace`

### Client Methods (Server → Client)
| Method | Payload | Description |
|--------|---------|-------------|
| `OnFlightStarted` | `FlightSessionDto` | Flight started |
| `OnFlightEnded` | `Guid sessionId` | Flight ended |
| `OnPositionUpdated` | `FlightPositionUpdate` | Position broadcast |
| `OnEncounterDetected` | `EncounterNotification` | Encounter started |
| `OnEncounterResult` | `EncounterResultNotification` | Encounter resolved |
| `OnBirdStatusChanged` | `BirdSummaryDto` | Bird state changed |
| `OnError` | `string message` | Error notification |

### Server Methods (Client → Server)
| Method | Parameters | Description |
|--------|------------|-------------|
| `RegisterPlayer` | `Guid playerId` | Register connection |
| `JoinAirspace` | `Guid sessionId` | Join flight group |
| `LeaveAirspace` | `Guid sessionId` | Leave flight group |
| `UpdatePosition` | `FlightPositionUpdate` | Send position |
