# Taklaci Guvercin - Game Mechanics

## Core Loop

```
Acquisition & Care → Genetic Breeding → Flight (Salınım) → Airspace Encounters
```

---

## Bird System

### Bird States
| State | Description |
|-------|-------------|
| `InCoop` | Resting in coop, can breed/fly |
| `Flying` | Active in airspace |
| `Sick` | Cannot perform actions until healed |
| `Resting` | Recovering stamina |

### Bird Stats (1-100)
| Stat | Effect |
|------|--------|
| **Leadership** | Team power bonus, encounter success |
| **Loyalty** | Risk reduction for losing bird |
| **Speed** | Flight duration, evasion chance |
| **GeneticDominance** | Breeding trait inheritance |

### Bird Rarity
| Rarity | Power Multiplier |
|--------|------------------|
| Common | 1.0x |
| Uncommon | 1.15x |
| Rare | 1.35x |
| Epic | 1.6x |
| Legendary | 1.85x |
| Mythical | 2.0x |

---

## Genetic System

### DNA Structure
Each bird has genes for:
- **Visual Traits:** PrimaryColor, SecondaryColor, Pattern, TailType, CrestType
- **Hidden Traits:** HiddenTrait1, HiddenTrait2 (affect breeding outcomes)
- **Element:** Fire, Ice, Wind, Emerald, or None

### Mendelian Inheritance
- Each gene has two alleles (one from each parent)
- Dominant alleles express over recessive
- Homozygous = same alleles, Heterozygous = different alleles

### Breeding Rules
1. Both parents must be in `InCoop` state
2. Both need Health > 50 and Stamina > 30
3. Offspring inherits one allele from each parent per gene
4. Mutation chance: 5-30% based on parents' mutation factor

### Element Matrix
```
Fire   beats Ice
Ice    beats Wind
Wind   beats Emerald
Emerald beats Fire
```

---

## Flight System

### Starting a Flight
- Select 1-5 birds from coop
- Birds must have Stamina >= 20
- Choose flight duration (10-60 minutes)
- Provide GPS coordinates (latitude/longitude)

### During Flight
- Position updates broadcast to all active flights
- Birds within 500m range trigger encounters
- Multiple encounters possible per flight

### Flight End
- Automatic when duration expires
- Manual via API
- Birds return to coop
- Stamina consumed (20 per flight)

---

## Encounter System

### Detection
- Background service scans every 5 seconds
- Two flights within 500m trigger encounter
- Both players notified via SignalR

### Combat Resolution

**1. Power Calculation**
```
BirdPower = (Leadership + Loyalty + Speed + GeneticDominance)
            × HealthMultiplier (0.5-1.0)
            × StaminaMultiplier (0.7-1.0)
            × RarityBonus (1.0-2.0)

FlockPower = Sum(BirdPower)
             × ElementAdvantage (up to +10% per bird)
             × FlockSynergy (+5% per same-element bird)
             × LeadershipBonus (up to +20%)
```

**2. Winner Determination**
```
WinChance = YourPower / (YourPower + OpponentPower)
RandomRoll = Random(0-100)
Winner = RandomRoll <= WinChance ? You : Opponent
```

**3. Loot Distribution**
```
LossPercentage = 10% + min(30%, (PowerRatio - 1) × 15%)
BirdsLost = ceil(LoserBirdCount × LossPercentage)
// Minimum 1 bird lost, but always keep at least 1
```

### Rewards

**Coins:**
```
BaseReward = 50
PowerBonus = LoserPower × 0.5
LevelBonus = (LoserLevel - WinnerLevel) × 25 (if applicable)
TotalCoins = BaseReward + PowerBonus + LevelBonus
```

**Experience:**
```
WinnerXP = min(500, BaseXP × DifficultyBonus + LoserPower × 0.1)
LoserXP = BaseXP / 2 (participation reward)
```

### Timeout Handling
- Encounters auto-resolve after 30 seconds
- Offline players can still lose birds
- Results sent via SignalR notification

---

## Player Progression

### Level System
```
XP Required = Level × 100
Level Up Bonus: +2 Coop Capacity
```

### Starting Resources
- 1000 Coins
- 10 Coop Capacity
- Level 1

### Stats Tracked
- Total Birds Owned
- Encounters Won/Lost
- Birds Looted/Lost

---

## Economy

### Currency
| Type | Earn Method | Spend On |
|------|-------------|----------|
| Coins | Encounters, Selling Birds | Breeding, Items |
| Premium | Real Money | Cosmetics, Boosts |

### Bird Value (Coins)
```
BaseValue = Rarity × 100
StatBonus = TotalPower × 0.5
LineageBonus = Generation × 10
```
