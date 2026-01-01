# Taklaci Guvercin - Architecture Overview

## Clean Architecture

The backend follows Clean Architecture (Onion Architecture) with 5 distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                          API Layer                          │
│              (Controllers, Program.cs, DTOs)                │
├─────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                     │
│     (EF Core, Repositories, SignalR, Background Services)   │
├─────────────────────────────────────────────────────────────┤
│                     Application Layer                       │
│           (Service Interfaces, Business Logic)              │
├─────────────────────────────────────────────────────────────┤
│                       Domain Layer                          │
│        (Entities, Value Objects, Enums, Domain Logic)       │
├─────────────────────────────────────────────────────────────┤
│                       Shared Layer                          │
│              (DTOs, Enums for Unity Client)                 │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
Backend/
├── TaklaciGuvercin.Api/                 # ASP.NET Core Web API
│   ├── Controllers/                     # REST API endpoints
│   ├── Program.cs                       # App entry point
│   └── appsettings.json                 # Configuration
│
├── TaklaciGuvercin.Application/         # Business Logic Layer
│   ├── Interfaces/                      # Service & Repository contracts
│   └── Services/                        # Business logic implementations
│
├── TaklaciGuvercin.Domain/              # Core Domain Layer
│   ├── Common/                          # Base classes
│   ├── Entities/                        # Domain entities
│   ├── Enums/                           # Domain enums
│   └── ValueObjects/                    # Value objects (DNA, Stats, Gene)
│
├── TaklaciGuvercin.Infrastructure/      # Data & External Services
│   ├── Data/                            # DbContext, Migrations, Seeder
│   ├── Repositories/                    # Repository implementations
│   ├── Hubs/                            # SignalR hubs
│   └── BackgroundServices/              # Hosted services
│
└── TaklaciGuvercin.Shared/              # Shared with Unity
    ├── DTOs/                            # Data Transfer Objects
    ├── Enums/                           # Shared enums
    └── Common/                          # Result wrapper
```

## Layer Dependencies

```
API → Application, Infrastructure, Shared
Infrastructure → Application, Domain, Shared
Application → Domain, Shared
Domain → (no dependencies)
Shared → (no dependencies, targets .NET Standard 2.1)
```

## Key Patterns

### Repository Pattern
- Generic `IRepository<T>` base interface
- Specialized repositories (IBirdRepository, IPlayerRepository, etc.)
- Unit of Work for transaction management

### Service Pattern
- `IEncounterService` - Combat and encounter logic
- `IFlightManagementService` - Flight lifecycle management

### Background Services
- `FlightExpirationService` - Auto-expires flights every 30s
- `EncounterDetectionService` - Scans for encounters every 5s

### Real-Time Communication
- SignalR `AirspaceHub` for push notifications
- Typed client interface `IAirspaceClient`

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8.0 |
| Database | PostgreSQL |
| ORM | Entity Framework Core 8.0 |
| Real-Time | SignalR |
| API Docs | Swagger/OpenAPI |
| Shared Library | .NET Standard 2.1 (Unity compatible) |
