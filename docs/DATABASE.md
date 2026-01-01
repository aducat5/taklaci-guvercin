# Taklaci Guvercin - Database Schema

## Database Info
- **Engine:** PostgreSQL
- **ORM:** Entity Framework Core 8.0
- **Connection:** `Host=localhost;Port=5432;Database=taklaci_guvercin`

---

## Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────┐
│   Player    │       │    Bird     │
├─────────────┤       ├─────────────┤
│ Id (PK)     │◄──────┤ OwnerId     │
│ Username    │       │ Id (PK)     │
│ Email       │       │ MotherId    │───┐
│ PasswordHash│       │ FatherId    │───┤
│ Coins       │       │ DNA (owned) │   │
│ Level       │       │ Stats(owned)│   │
│ ...         │       │ ...         │◄──┘
└─────────────┘       └─────────────┘
      │                     │
      │                     │
      ▼                     ▼
┌─────────────┐       ┌─────────────┐
│FlightSession│       │  Encounter  │
├─────────────┤       ├─────────────┤
│ Id (PK)     │◄──────┤InitSessionId│
│ PlayerId    │       │TargetSessId │
│ BirdIds     │       │InitPlayerId │
│ Lat/Long    │       │TargetPlayerId│
│ IsActive    │       │ State       │
│ ...         │       │ WinnerId    │
└─────────────┘       │ LootedBirds │
                      └─────────────┘
```

---

## Tables

### Players
```sql
CREATE TABLE "Players" (
    "Id" uuid PRIMARY KEY,
    "Username" varchar(50) NOT NULL UNIQUE,
    "Email" varchar(256) NOT NULL UNIQUE,
    "PasswordHash" varchar(512) NOT NULL,
    "Coins" integer NOT NULL DEFAULT 1000,
    "PremiumCurrency" integer NOT NULL DEFAULT 0,
    "TotalBirdsOwned" integer NOT NULL DEFAULT 0,
    "TotalEncountersWon" integer NOT NULL DEFAULT 0,
    "TotalEncountersLost" integer NOT NULL DEFAULT 0,
    "TotalBirdsLost" integer NOT NULL DEFAULT 0,
    "TotalBirdsLooted" integer NOT NULL DEFAULT 0,
    "Level" integer NOT NULL DEFAULT 1,
    "Experience" integer NOT NULL DEFAULT 0,
    "CoopCapacity" integer NOT NULL DEFAULT 10,
    "LastLoginAt" timestamp NOT NULL,
    "IsOnline" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NULL
);

CREATE INDEX "IX_Players_Email" ON "Players" ("Email");
CREATE INDEX "IX_Players_Username" ON "Players" ("Username");
```

### Birds
```sql
CREATE TABLE "Birds" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "OwnerId" uuid NOT NULL,
    "State" varchar(50) NOT NULL,
    "Rarity" varchar(50) NOT NULL,

    -- Stats (owned entity)
    "Leadership" integer NOT NULL,
    "Loyalty" integer NOT NULL,
    "Speed" integer NOT NULL,
    "GeneticDominance" integer NOT NULL,

    -- DNA (owned entity with nested genes)
    "Element" varchar(50) NOT NULL,
    "MutationFactor" real NOT NULL,
    "PrimaryColor_TraitName" varchar(50),
    "PrimaryColor_Allele1" varchar(50),
    "PrimaryColor_Allele2" varchar(50),
    "PrimaryColor_Type" varchar(50),
    -- ... (repeat for all 8 genes)

    -- Lineage
    "MotherId" uuid NULL,
    "FatherId" uuid NULL,
    "Generation" integer NOT NULL DEFAULT 1,

    -- Health
    "Health" integer NOT NULL DEFAULT 100,
    "MaxHealth" integer NOT NULL DEFAULT 100,
    "Stamina" integer NOT NULL DEFAULT 100,
    "MaxStamina" integer NOT NULL DEFAULT 100,
    "LastFedAt" timestamp NULL,
    "SickUntil" timestamp NULL,
    "RestingUntil" timestamp NULL,

    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NULL
);

CREATE INDEX "IX_Birds_OwnerId" ON "Birds" ("OwnerId");
CREATE INDEX "IX_Birds_State" ON "Birds" ("State");
```

### FlightSessions
```sql
CREATE TABLE "FlightSessions" (
    "Id" uuid PRIMARY KEY,
    "PlayerId" uuid NOT NULL,
    "BirdIds" text NOT NULL, -- Comma-separated GUIDs
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "Altitude" double precision NOT NULL DEFAULT 100,
    "StartedAt" timestamp NOT NULL,
    "EndedAt" timestamp NULL,
    "Duration" interval NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "EncountersCount" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NULL
);

CREATE INDEX "IX_FlightSessions_PlayerId" ON "FlightSessions" ("PlayerId");
CREATE INDEX "IX_FlightSessions_IsActive" ON "FlightSessions" ("IsActive");
CREATE INDEX "IX_FlightSessions_Location" ON "FlightSessions" ("Latitude", "Longitude");
```

### Encounters
```sql
CREATE TABLE "Encounters" (
    "Id" uuid PRIMARY KEY,
    "InitiatorSessionId" uuid NOT NULL,
    "TargetSessionId" uuid NOT NULL,
    "InitiatorPlayerId" uuid NOT NULL,
    "TargetPlayerId" uuid NOT NULL,
    "State" varchar(50) NOT NULL, -- Pending, InProgress, Resolved, Cancelled
    "WinnerPlayerId" uuid NULL,
    "LootedBirdIds" text NOT NULL DEFAULT '', -- Comma-separated GUIDs
    "CoinsLooted" integer NOT NULL DEFAULT 0,
    "InitiatorPower" integer NOT NULL DEFAULT 0,
    "TargetPower" integer NOT NULL DEFAULT 0,
    "RandomRoll" integer NOT NULL DEFAULT 0,
    "ResolvedAt" timestamp NULL,
    "InitiatorWasOnline" boolean NOT NULL,
    "TargetWasOnline" boolean NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NULL
);

CREATE INDEX "IX_Encounters_InitiatorPlayerId" ON "Encounters" ("InitiatorPlayerId");
CREATE INDEX "IX_Encounters_TargetPlayerId" ON "Encounters" ("TargetPlayerId");
CREATE INDEX "IX_Encounters_State" ON "Encounters" ("State");
```

---

## Value Object Mapping

### BirdStats (Owned by Bird)
Stored as columns in Birds table:
- `Leadership`, `Loyalty`, `Speed`, `GeneticDominance`

### BirdDNA (Owned by Bird)
Stored as columns in Birds table:
- `Element`, `MutationFactor`
- 8 genes, each with `_TraitName`, `_Allele1`, `_Allele2`, `_Type` columns

### Gene (Owned by BirdDNA)
Each gene becomes 4 columns:
- `{GeneName}_TraitName`
- `{GeneName}_Allele1`
- `{GeneName}_Allele2`
- `{GeneName}_Type`

---

## Migrations

### Create Initial Migration
```bash
cd Backend
dotnet ef migrations add InitialCreate \
  -p TaklaciGuvercin.Infrastructure \
  -s TaklaciGuvercin.Api
```

### Apply Migration
```bash
dotnet ef database update \
  -p TaklaciGuvercin.Infrastructure \
  -s TaklaciGuvercin.Api
```

### Generate SQL Script
```bash
dotnet ef migrations script \
  -p TaklaciGuvercin.Infrastructure \
  -s TaklaciGuvercin.Api \
  -o migration.sql
```
