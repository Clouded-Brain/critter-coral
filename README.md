# Coral Critter

Prototype repository for **Coral Critter**, a creature-collection survival game concept.

## Build Target (Current)
- Engine: **Unity 6 (6000.0 LTS)**
- Render Pipeline: **URP (Universal Render Pipeline)**
- Camera: **3D world with locked isometric gameplay camera**

## Repo Layout
- `GAME_CONCEPT.md` — product concept and direction.
- `docs/` — technical planning and production documents.
- `game/` — Unity project scaffold.

## Unity Version
Use Unity Editor version:
- `6000.0.34f1`

(Defined in `game/ProjectSettings/ProjectVersion.txt`.)

## Next Run Instructions
1. Open Unity Hub.
2. Add project from `game/`.
3. Let Unity generate `Library/` and local artifacts.
4. Start in `Assets/_Project/Scenes/Bootstrap.unity`.
5. Follow `docs/MVP_BACKLOG.md` and `docs/SPIKE_48H.md` for implementation order.


## First Implementation Steps (CC-001, CC-002, CC-003)
1. Open `Assets/_Project/Scenes/IslandPrototype.unity`.
2. Create `Player` GameObject:
   - Add `CharacterController`.
   - Add `PlayerMovementController`.
3. Select `Main Camera`:
   - Add `IsometricCameraController`.
   - Add `CameraOcclusionController`.
   - Drag the `Player` transform into the `IsometricCameraController.target` field.
4. Create `IslandGenerator` empty GameObject:
   - Add `IslandSeedPresetGenerator`.
   - Assign temporary terrain/water materials.
   - Click the component context menu `Generate`.
5. Press Play and validate:
   - WASD move + Shift sprint.
   - Scroll wheel zoom constraints.
   - Player remains readable behind blockers.
   - Island shape is stable across runs with same seed value.
