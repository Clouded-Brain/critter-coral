# Coral Critter — Concept Pitch

## Core Fantasy
A cozy-but-dangerous **creature collecting survival adventure** set across procedurally generated island clusters. Think: atmospheric exploration and building vibes similar to Valheim, but presented through an isometric camera with pixel-stylized art, plus creature tracking, net-catching, and auto-battler squad combat.

## Design Pillars
1. **Hunt, Don’t Throw**
   - Capturing creatures is an active chase-and-control skill challenge, not a button toss.
   - Players deploy nets, harpoons, traps, and lures.
2. **Living Archipelago**
   - Procedural islands with distinct biomes, weather, and creature ecologies.
   - Sailing between islands is part of progression.
3. **Tactical Squad Combat**
   - Battles resolve in an auto-battler style (positioning, synergies, pre-fight loadout choices).
   - Decision-making happens before and between rounds.
4. **World Shaping Progression**
   - Mid/late game unlocks terrain manipulation and habitat engineering.
   - You alter environments to attract, evolve, and farm specific creatures.

## Core Gameplay Loop
1. Scout a new island biome.
2. Track wild critters (footprints, sounds, disturbed flora).
3. Enter a net-hunt encounter (vehicle + on-foot hybrid chase).
4. Secure creatures and return to base.
5. Build habitats, train teams, and craft tactical loadouts.
6. Enter PvE expeditions / boss hunts / ranked gauntlets.
7. Unlock tools that let you reshape terrain and discover rarer species.

## The Capture System (Arcade Safari Inspiration)
### "Safari Rig" Encounters
- Player drives a customizable off-road safari truck/raft rig through a contained hunting zone.
- Creature has a **stamina + panic + awareness** model.
- Goal: maintain ideal pursuit distance and direction while deploying net tech.

### Capture Steps
1. **Locate:** Follow tracks, calls, and thermal signatures.
2. **Shadow:** Stay in an optimal cone behind or beside the target to build "Net Control".
3. **Pressure:** Use horn, drone, lure pods, or terrain cuts to herd creature.
4. **Commit:** Fire weighted net; success depends on control meter, creature state, and net type.
5. **Secure:** Mini-sequence to stabilize capture (tighten cables, sedative timing, calm pulses).

### Why this feels different
- Mechanical chase skill replaces random catch probability.
- Different creatures require different pursuit patterns.
- Vehicle handling upgrades become meaningful progression.

## Combat System (Auto Chess / TFT Feel, but Adventure-Driven)
### Squad Setup
- Team size: 3–6 active critters.
- Pre-battle choices:
  - Formation grid positions.
  - Role runes/items.
  - Team trait activation (e.g., Reef Pack, Stormborn, Burrowline).

### Battle Flow
- Real-time auto-combat rounds (~20–40 seconds).
- Player can trigger a limited number of tactical commands per fight (one interrupt, one reposition pulse, one ult focus mark).
- Victory rewards materials, mutation shards, and biome intel.

### Creature Identity
- Each species has:
  - Primary role (Bruiser, Disruptor, Sniper, Support, Controller).
  - Trait family (biome/ecology based).
  - Signature behavior that changes with terrain.

## Progression Structure
### Early Game
- One starter creature and basic hand-net.
- Small home island, simple workbench, short-range scouting.

### Mid Game
- Boat + Safari Rig upgrades.
- Multi-island expeditions.
- First major habitat engineering systems (irrigation, heat vents, soil tuning).

### Late Game
- Terrain manipulation tools (raise/lower land, carve channels, seed micro-biomes).
- Legendary migratory creature events.
- Meta-build crafting for high-tier auto-battler play.

## Procedural Islands (Practical MVP Approach)
1. **Biome tiles** generated from layered noise + climate map.
2. **Landmarks** injected via rules (ruins, caves, geysers, giant trees).
3. **Ecology pass** assigns creature spawn networks and predator-prey links.
4. **Resource pass** places crafting nodes and traversal blockers.
5. **Dynamic events** (storms, migrations, infestations) alter island behavior over time.

## Base Building + Terrain Manipulation (Valheim-Like Direction)
- Start with modular base parts (wood, stone, coral composite).
- Habitats affect creature mood, growth, and combat perks.
- Terrain tools unlock gradually to avoid early complexity overload:
  1. Flatten and reinforce.
  2. Dig channels and ponds.
  3. Raise berms and cliffs.
  4. Paint biome modifiers (humidity, salinity, temperature).

## Camera + Visual Art Direction
- Isometric gameplay camera (with slight pitch/zoom controls) to keep tactical readability during exploration and battles.
- Pixel-stylized world assets with modern lighting, fog, and shadows for a "retro detail + moody atmosphere" blend.
- Chunkier silhouettes and high-contrast palettes so creatures are readable at isometric distance.
- Weather and biome color grading signal danger levels and creature behavior shifts.
- Animation style: snappy, arcade-like anticipation in chase moments; smoother idle cycles in base zones.

## Tone and Aesthetic
- Moody skies, stylized pixel-realism, and grounded materials.
- Creatures are expressive but not cartoon mascots.
- Soundscape emphasizes wind, surf, engine rattle, distant calls.

## Multiplayer Possibilities
- Co-op island expeditions (2–4 players).
- Shared habitat ranching.
- Asynchronous ghost routes for capture challenges.
- Optional PvP auto-battle league using captured squads.



## 2D vs 3D + Engine Recommendation
### Short Answer
- Build this as **3D gameplay with an isometric camera**, then use stylized/pixel-inspired rendering.
- Best default engine choice: **Unity**.
- Strong alternative (especially if team is small and code-heavy): **Godot 4**.

### Why 3D fits your specific game
- Terrain manipulation is dramatically easier and more believable in 3D heightfields/meshes.
- Procedural islands, elevation, cliffs, and water interactions are more natural in a 3D world.
- Your safari chase fantasy (vehicle pursuit + pathing + visibility) benefits from depth and verticality.
- Auto-battle readability still works with a locked isometric camera and constrained arena layouts.

### When 2D could still be right
Choose 2D only if your top priority is faster content production and lower technical risk, and you're willing to simplify:
- No true terrain sculpting (or only tile-height tricks).
- Simpler capture chase behaviors and movement.
- Less emphasis on dynamic camera/occlusion.

### Engine Decision Matrix (for this project)
1. **Unity (Recommended default)**
   - Pros: mature tooling, strong 3D/isometric workflows, broad middleware ecosystem, good profiling/debugging.
   - Cons: heavier editor/runtime footprint, licensing/business terms should be reviewed early.
   - Best if: you want fastest path to a production-ready 3D vertical slice with flexibility.

2. **Godot 4 (Great indie alternative)**
   - Pros: lightweight, open source, fast iteration, clean scripting workflow.
   - Cons: fewer off-the-shelf production plugins than Unity for some pipelines.
   - Best if: small team, engineer-led workflow, cost/control are priorities.

3. **Unreal (Not ideal for your MVP)**
   - Pros: top-tier visuals and world tooling.
   - Cons: overkill complexity for this stylized indie-scope prototype.
   - Best if: you already have strong Unreal experience and aim for higher-fidelity 3D from day one.

### Practical Recommendation for you
- **Pick Unity + 3D isometric now** for MVP.
- Art approach: 3D assets with pixel-stylized textures/post-processing, not pure sprite-only 2D.
- Camera rules for MVP:
  1. Fixed isometric angle.
  2. Limited zoom only.
  3. No free rotation.
  4. Strong silhouette contrast and simple shadowing for combat readability.

### 48-Hour Technical Spike (to validate the choice)
Build a throwaway prototype and verify these before full production:
1. Generate one tiny island chunk with elevation + water.
2. Move player + one creature using the locked isometric camera.
3. Run one net chase interaction with a visible control meter.
4. Trigger one 3v3 auto-battle in a separate arena.
5. Record frame time and readability notes.

If these five checks feel good, you have your stack confirmed.

## Where to Start (Actionable First Steps)
### Step 1: Lock the "Fun First" Vertical Slice (1–2 days)
- Freeze the first playable target: **one small island, one capture chase, one 3v3 auto-battle**.
- Write a one-page success checklist:
  - "Chasing feels tense and readable."
  - "Net capture feels skill-based, not random."
  - "Auto-battle outcome is understandable from formation + traits."

### Step 2: Choose Prototype Tech + Camera Rules (day 2)
- Pick engine (Unity or Godot are both viable for isometric + stylized pipelines).
- Lock camera constraints early:
  - Fixed isometric angle with limited zoom.
  - No free-rotate in MVP (preserves readability and scope).
  - Silhouette/readability test scene before content production.

### Step 3: Build the Capture Loop First (week 1)
- Implement one creature archetype (runner type) with simple AI states: roam -> alerted -> flee -> exhausted.
- Implement the **Net Control meter** and a single weighted-net tool.
- Build one 2–3 minute hunt encounter on a tiny test island.
- Playtest question: "Did player skillful pursuit clearly increase catch success?"

### Step 4: Add Minimal Auto-Battle Loop (week 2)
- Create 6 prototype creatures with placeholder art and clear roles.
- Build a tiny pre-battle formation grid and one trait synergy.
- Run battles with deterministic logging to tune readability and balance.
- Playtest question: "Could players explain why they won/lost?"

### Step 5: Connect Both Loops (week 3)
- Captured creatures immediately become usable in auto-battle roster.
- Win battles to earn upgrade materials for rig/net improvements.
- This closes your core fantasy loop: **hunt -> collect -> deploy -> upgrade -> hunt harder targets**.

### Step 6: Define Team Workflow (start now)
- Daily build target: shippable internal playable every day.
- Keep strict placeholder policy for first month (no polish rabbit holes).
- Track only three metrics at first:
  1. Time-to-first-capture.
  2. Capture success rate by creature type.
  3. Battle clarity score from testers (1–5).

### Recommended Immediate Backlog (next 10 tasks)
1. Input + movement controller.
2. Isometric camera controller with occlusion handling.
3. One procedural island seed preset (not full procedural system yet).
4. Creature AI state machine (runner archetype).
5. Net projectile + hit resolution.
6. Net Control meter UI.
7. Capture outcome and roster storage.
8. 3x3 formation screen.
9. Auto-battle simulation tick + combat log.
10. End-of-loop rewards that upgrade chase tools.

### First Playtest Milestone (end of week 3)
- Internal demo where a new tester can:
  1. Catch one creature in under 10 minutes.
  2. Build a 3-creature team.
  3. Win one battle and understand why.
- If this works, scale content. If this fails, iterate mechanics before adding more features.

## MVP Slice (What to build first)
1. One procedural island seed with 2 biomes.
2. 8–12 creature species.
3. Safari rig chase capture loop for 3 species archetypes.
4. 3v3 auto-battler combat prototype with traits + formations.
5. Basic home base and creature storage.

## Example Creature Concepts
- **Brumblehog** (Burrow Bruiser): rolls to knock back frontliners, stronger on soft soil.
- **Kelp Kite** (Aerial Disruptor): dive-bombs and blinds, stronger during rain.
- **Ember Manta** (Support Controller): emits heat waves that buff allies and create burn zones.

## Simple 6-Month Prototype Plan
### Month 1–2: Core Systems
- Movement, isometric camera controller, island generation testbed, creature AI state machine.
- Placeholder net chase with one target creature.

### Month 3–4: Fun Validation
- Add safari rig handling + capture meter.
- Add first auto-battle loop with 6 creatures.
- Run playtests focused on: "Is chasing and catching satisfying?"

### Month 5–6: Vertical Slice
- One polished island biome pair.
- 10 creatures, 2 minibosses, and one end-of-island boss.
- Basic building + habitat bonuses.
- Save/load + progression economy.

## Pitch One-Liner
**"A survival creature-collector where you hunt with nets across procedural islands, then deploy your captured beasts in tactical auto-battles while gradually reshaping the world itself."**
