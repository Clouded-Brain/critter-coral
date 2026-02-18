# Coral Critter — MVP Backlog

Milestones are ordered for earliest playable loop.

## M1 — Capture Loop

### CC-001 Input + Movement Controller
- Owner: Engineering
- Effort: M
- Dependencies: none
- Definition of done:
  - Character moves in 3D world space from keyboard/gamepad input.
  - Sprint toggle available.
- Acceptance test:
  - Manual: move through IslandPrototype for 60 seconds without control lock.

### CC-002 Isometric Camera Controller + Occlusion Pass
- Owner: Engineering
- Effort: M
- Dependencies: CC-001
- Definition of done:
  - Fixed-angle follow camera with constrained zoom.
  - Basic occlusion handling for large props (fade/dither).
- Acceptance test:
  - Manual: player remains visible while moving behind at least 3 blocking props.

### CC-003 Island Seed Preset (Single Prototype Seed)
- Owner: Engineering + Design
- Effort: M
- Dependencies: none
- Definition of done:
  - One deterministic island seed generated with shoreline + elevation.
- Acceptance test:
  - Manual: seed ID loads same terrain shape in 5/5 launches.

### CC-004 Creature Runner AI State Machine
- Owner: Engineering
- Effort: M
- Dependencies: CC-001, CC-003
- Definition of done:
  - AI supports roam -> alerted -> flee -> exhausted transitions.
- Acceptance test:
  - Manual: QA can trigger each state in one play session.

### CC-005 Net Projectile + Hit Resolution
- Owner: Engineering
- Effort: S
- Dependencies: CC-004
- Definition of done:
  - Player can fire weighted net; valid hit binds target for resolution.
- Acceptance test:
  - Manual: 10 net throws produce expected hit/miss feedback.

### CC-006 Net Control Meter UI
- Owner: Engineering + UI
- Effort: S
- Dependencies: CC-004, CC-005
- Definition of done:
  - Meter visible and updates in real time during chase.
- Acceptance test:
  - Manual: meter rises in optimal pursuit band and drops when line is broken.

### CC-007 Capture Outcome + Roster Storage
- Owner: Engineering
- Effort: M
- Dependencies: CC-005, CC-006
- Definition of done:
  - Successful capture writes creature to local roster.
- Acceptance test:
  - Manual: captured creature appears in roster and persists after restart.

## M2 — Auto-Battle Core

### CC-008 3x3 Formation Screen
- Owner: Engineering + UI
- Effort: M
- Dependencies: CC-007
- Definition of done:
  - Assign creatures to 3x3 grid with saveable layout.
- Acceptance test:
  - Manual: assign 3 creatures and launch battle with positions preserved.

### CC-009 Auto-Battle Simulation Tick + Combat Log
- Owner: Engineering
- Effort: L
- Dependencies: CC-008
- Definition of done:
  - Deterministic 3v3 sim runs at fixed tick.
  - Emits combat log lines for every action.
- Acceptance test:
  - Manual: repeated run with same seed produces same winner and log checksum.

## M3 — Loop Integration

### CC-010 End-of-Loop Rewards for Chase Tool Upgrades
- Owner: Engineering + Design
- Effort: M
- Dependencies: CC-009
- Definition of done:
  - Winning battle grants materials used to upgrade net/rig stats.
- Acceptance test:
  - Manual: complete hunt->battle->upgrade flow in one session.
