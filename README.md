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


## One-Click Test Scene Setup
If you want a fast playable setup in `IslandPrototype`:
1. Open `Assets/_Project/Scenes/IslandPrototype.unity`.
2. Create an empty GameObject named `PrototypeSetup`.
3. Add `PrototypeSceneSetup` component.
4. In the component context menu, click `Create/Refresh Test Setup`.
5. Press Play.

This auto-creates:
- `Player` (capsule + CharacterController + PlayerMovementController)
- `Main Camera` with IsometricCameraController + CameraOcclusionController
- `IslandGenerator` with IslandSeedPresetGenerator (deterministic seed)


## Troubleshooting
- **Camera does not follow player:**
  - Re-run `PrototypeSceneSetup -> Create/Refresh Test Setup`.
  - Confirm `Main Camera` has `IsometricCameraController` and its target is `Player`.
- **Player does not move:**
  - Movement now supports both legacy and new input system keyboard controls (WASD + Shift).
- **Terrain is pink:**
  - The generator now creates fallback materials automatically and prefers URP shaders.
  - If pink persists, verify URP package import completed and re-run `Generate`.
- **Player falls through map:**
  - The generated island now includes a `MeshCollider`; re-run setup if scene was created before this update.
