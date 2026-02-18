# Coral Critter — 48-Hour Technical Spike Runbook

Purpose: validate the chosen stack (Unity + 3D isometric) before deeper production.

## Scope
Timebox: 2 days
Team size assumption: 1–3 developers
Output: go/no-go decision with evidence

## Spike Goals and Pass/Fail Criteria

### Goal A — Tiny Island Chunk (Elevation + Water)
- Pass criteria:
  - Terrain chunk generated from fixed seed.
  - Includes at least 2 elevation bands and a water boundary.
- Fail if:
  - Terrain output is unstable across repeated launches with same seed.

### Goal B — Isometric Movement Readability
- Pass criteria:
  - Player + one creature move smoothly under locked isometric camera.
  - Player silhouette remains readable in all test routes.
- Fail if:
  - Occlusion/camera causes player loss for >2 seconds in routine navigation.

### Goal C — Net Chase Interaction
- Pass criteria:
  - One chase encounter starts and resolves with visible Net Control meter.
  - Capture chance clearly changes with pursuit quality.
- Fail if:
  - Outcome appears random despite different chase quality.

### Goal D — 3v3 Auto-Battle Prototype
- Pass criteria:
  - Battle launches from roster with simple formation input.
  - Deterministic winner for fixed seed and lineup.
- Fail if:
  - Same seed/lineup produces inconsistent outcomes.

### Goal E — Performance + Readability Snapshot
- Pass criteria:
  - Stable gameplay at target minimum 60 FPS on dev machine.
  - Testers rate clarity >= 4/5 for chase + battle understanding.
- Fail if:
  - FPS unstable below 45 for extended windows or clarity < 3/5.

## Evidence Collection
For each goal attach:
- short video/gif capture
- profiler/frame-time screenshot
- notes on blockers and workaround used

## Readability Feedback Form (1–5)
- Camera clarity in exploration: [ ]
- Camera clarity in chase: [ ]
- Creature readability at distance: [ ]
- Understanding of battle outcomes: [ ]
- UI comprehension (net meter + formation): [ ]

## Performance Capture Fields
- Hardware: 
- Resolution: 
- Average FPS: 
- 1% low FPS: 
- Avg frame time (ms): 
- Peak frame time (ms): 

## Decision Rubric
Choose **GO** if:
- Goals A-D pass.
- Goal E mostly passes (or has clear low-risk mitigations).
- No blocker rated Critical remains unresolved.

Choose **NO-GO** if:
- Any of A-D fails with no immediate mitigation.
- Spike reveals major architectural mismatch for terrain/chase/battle loop.

## Spike Result Template (Fill After Run)
- Date:
- Team:
- Build hash:
- Goal A: Pass/Fail + notes
- Goal B: Pass/Fail + notes
- Goal C: Pass/Fail + notes
- Goal D: Pass/Fail + notes
- Goal E: Pass/Fail + notes
- Critical blockers:
- Final decision: GO / NO-GO
- Follow-up actions:
