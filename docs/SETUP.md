# Taklaci Guvercin - Development Setup

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Unity 2022.3 LTS](https://unity.com/releases/editor/qa/lts-releases) (for client)
- Git

---

## Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/aducat5/taklaci-guvercin.git
cd taklaci-guvercin
```

### 2. PostgreSQL Setup
```bash
# Create database
psql -U postgres -c "CREATE DATABASE taklaci_guvercin;"
```

### 3. Update Connection String
Edit `Backend/TaklaciGuvercin.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taklaci_guvercin;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 4. Install EF Core Tools
```bash
dotnet tool install -g dotnet-ef
```

### 5. Apply Migrations
```bash
cd Backend
dotnet ef database update -p TaklaciGuvercin.Infrastructure -s TaklaciGuvercin.Api
```

### 6. Run the API
```bash
dotnet run --project TaklaciGuvercin.Api
```

API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

---

## Project Structure

```
taklaci-guvercin/
├── Backend/                    # .NET Backend
│   ├── TaklaciGuvercin.Api/
│   ├── TaklaciGuvercin.Application/
│   ├── TaklaciGuvercin.Domain/
│   ├── TaklaciGuvercin.Infrastructure/
│   ├── TaklaciGuvercin.Shared/
│   └── TaklaciGuvercin.sln
├── Assets/                     # Unity Project
├── docs/                       # Documentation
└── .gitignore
```

---

## Common Commands

### Build
```bash
cd Backend
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Watch Mode (Hot Reload)
```bash
dotnet watch --project TaklaciGuvercin.Api run
```

### Create Migration
```bash
dotnet ef migrations add MigrationName \
  -p TaklaciGuvercin.Infrastructure \
  -s TaklaciGuvercin.Api
```

### Remove Last Migration
```bash
dotnet ef migrations remove \
  -p TaklaciGuvercin.Infrastructure \
  -s TaklaciGuvercin.Api
```

### Reset Database
```bash
dotnet ef database drop -p TaklaciGuvercin.Infrastructure -s TaklaciGuvercin.Api
dotnet ef database update -p TaklaciGuvercin.Infrastructure -s TaklaciGuvercin.Api
```

---

## Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taklaci_guvercin;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Environment Variables (Alternative)
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;..."
```

---

## Development Tips

### Enable Detailed Errors
In `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### View SQL Queries
EF Core logs SQL when LogLevel is Information or Debug.

### Test SignalR
Use [SignalR Test Client](https://github.com/nickvane/SignalRTest) or Postman.

---

## Unity Integration

### Add Shared Library
1. Build the Shared project:
   ```bash
   cd Backend/TaklaciGuvercin.Shared
   dotnet build -c Release
   ```
2. Copy `bin/Release/netstandard2.1/TaklaciGuvercin.Shared.dll` to Unity's `Assets/Plugins/`

### Install SignalR Client
Add to Unity via NuGet:
- `Microsoft.AspNetCore.SignalR.Client`

Or use Unity Package Manager with OpenUPM.

---

## Troubleshooting

### Database Connection Failed
1. Verify PostgreSQL is running: `pg_isready`
2. Check connection string
3. Ensure database exists: `psql -U postgres -c "\l"`

### Migration Failed
1. Check for model changes
2. Rebuild solution: `dotnet build`
3. Try removing last migration: `dotnet ef migrations remove`

### SignalR Not Connecting
1. Check CORS settings in Program.cs
2. Verify WebSocket support
3. Check browser/client firewall

### Port Already in Use
```bash
# Find process on port 5000
netstat -ano | findstr :5000
# Kill process
taskkill /PID <PID> /F
```

---

## Production Deployment

### Environment
```bash
export ASPNETCORE_ENVIRONMENT=Production
```

### Build Release
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "TaklaciGuvercin.Api.dll"]
```

---

## Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [EF Core Docs](https://docs.microsoft.com/ef/core)
- [SignalR Docs](https://docs.microsoft.com/aspnet/core/signalr)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
