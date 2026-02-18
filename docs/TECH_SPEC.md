# Coral Critter — Technical Specification (MVP)

This document maps the concept in `GAME_CONCEPT.md` to implementation contracts for the first playable build.

## 1) Capture Loop System Contract
Related concept sections:
- `The Capture System (Arcade Safari Inspiration)`
- `Where to Start -> Step 3`

### 1.1 Creature Capture State Machine
```text
roam -> alerted -> flee -> exhausted -> captured
```

State definitions:
- `roam`: idle wandering and low-speed pathing inside habitat zone.
- `alerted`: creature has detected player/rig; begins evasive intent.
- `flee`: high-speed movement using escape waypoints.
- `exhausted`: stamina floor reached; capture resistance reduced.
- `captured`: net resolved and creature removed from wild simulation.

Transition events:
- `OnPlayerDetected`, `OnThreatLost`, `OnStaminaThresholdReached`, `OnNetHit`, `OnCaptureSecured`.

### 1.2 Net Control Meter (0–100)
Goal: reward sustained, skillful pursuit and reduce random-feel outcomes.

Formula (per simulation tick):
```text
NetControl += DistanceScore + AngleScore + PaceScore - PanicPenalty - TerrainPenalty
```

Tunable defaults:
- target band distance: 6m–14m
- ideal pursuit cone: ±40° behind target
- max meter gain/tick: +1.6
- max meter loss/tick: -2.0
- capture threshold for weighted net: 65+

Capture resolution:
```text
CaptureChance = clamp(BaseNetChance + (NetControl * 0.5) - (CreaturePanic * 0.3), 5, 95)
```

## 2) Creature Data Schema
Related concept sections:
- `Combat System -> Creature Identity`
- `MVP Slice`

Suggested serialized schema (`CreatureDefinition`):
- `id` (string)
- `displayName` (string)
- `role` (enum: Bruiser, Disruptor, Sniper, Support, Controller)
- `traitFamily` (enum/string)
- `baseStats`
  - `health`
  - `attack`
  - `defense`
  - `speed`
  - `energy`
- `captureProfile`
  - `baseAwareness`
  - `panicGainRate`
  - `staminaPool`
- `battleProfile`
  - `attackInterval`
  - `targetPriority`
  - `abilityId`
- `behaviorTags` (array<string>)

## 3) Auto-Battle Simulation Contract
Related concept sections:
- `Combat System`
- `Where to Start -> Step 4`

### 3.1 Deterministic Tick
- fixed simulation step: 10 Hz (0.1s)
- all combat decisions evaluated on fixed ticks
- RNG seeded by `BattleSeed`

### 3.2 Core Phases per Tick
1. Acquire target
2. Move/position update
3. Basic attack checks
4. Ability trigger checks
5. Damage/heal resolution
6. Death/cleanup events

### 3.3 Combat Log Format
Structured log line:
```text
[tick] actorId action targetId value tags
```
Example:
```text
[0142] brumblehog_01 BasicAttack kelp_kite_02 18 crit=false
```

## 4) Save Data Contract
Related concept sections:
- `Core Gameplay Loop`
- `Where to Start -> Step 5`

`SaveGame` root:
- `profileId`
- `playtimeSeconds`
- `capturedCreatures[]`
  - `creatureId`
  - `level`
  - `mutations[]`
  - `bond`
- `activeRoster[]` (3–6 entries)
- `rigUpgrades[]`
- `islandProgress`
  - `seed`
  - `unlockedBiomes[]`
  - `eventsCompleted[]`

## 5) Ownership and Implementation Sequence
1. Implement capture state machine + net meter.
2. Implement creature schema + 6 prototype entries.
3. Implement deterministic 3v3 battle tick.
4. Connect capture output to roster/save data.
5. Add metrics hooks for playtest scoring.
