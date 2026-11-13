---
name: standard-rooms
description: Explains how 1000 Suns room scenes are built and generates new rooms aligned to an existing room. Use when the user asks to create, add, generate, or set up a new room or scene next to another room, asks what the room types are (Standard, Horizontal, Vertical, Big room), or edits InitRoom, RoomMgr, Load, LevelEntry, SpawnPoint IDs, room cameras (MainVCam), or adjacent rooms. Not for gameplay scripting inside rooms, room-specific RoomManager logic, or background and walkable-surface scene content.
---

# Standard rooms (1000 Suns)

In this project one room is one Unity scene, and all rooms share one world space. The full reference is `/1000 suns demo/Assets/Editor/Rooms/README.md`. Read it with `str_replace_based_edit_tool` before you create or change a room. It covers room types, sizes and origins, required root objects and their wiring, pixel-grid rules, and legacy setups to avoid.

## Key facts (details in the README)
- 8 pixels per unit. One screen is 40 x 22.5 units. Every root object, especially `Load`, must be snapped to 0.125-unit steps.
- There are four biome-agnostic types (the "Cave" in the prefab names is historical). Each pairs a room prefab with a camera prefab from `Assets/Prefabs/level/scene_template/`:
  - Standard: `CaveStandardRoom` + `MainVCam static`
  - Horizontal: `CaveHorizontalRoom` + `MainVCam horizontal`
  - Vertical: `CaveVerticalRoom` + `MainVCam vertical`
  - Big: `CaveBigRoom` + `MainVCam big room`
- Never use the `.scenetemplate` files or the per-scene prefab copies under `Assets/Scenes/<Room>/`. They are leftovers from an abandoned experiment.
- The user resizes rooms by hand. Read a room's real bounds from its `Room`-tagged `PolygonCollider2D`, not from the standard sizes.

## Generating a new room
Use the project's custom action instead of building scenes action by action. It handles prefab instancing, pixel snapping, wiring, spawn IDs, adjacency, Build Settings and saving.

1. Get the signature with `getActionDefinition` for `RoomBeziActions_GenerateAlignedRoom`. It is a global, called as `await RoomBeziActions_GenerateAlignedRoom({ roomType, neighborScene, side, offsetInScreens, newSceneName })`, and returns `{ result: string }`. If the name has changed, `grep` `/1000 suns demo/.bezi/defs` for `GenerateAlignedRoom`. If the action is missing, Unity has not compiled `Assets/Editor/Rooms/` yet. Ask the user to focus the editor, then check `readConsole` with `shouldRecompile: true`.
2. Confirm the parameters with the user if they are unclear:
   - `roomType`: `Standard | Horizontal | Vertical | Big`.
   - `neighborScene`: for example `"Cave-60"`.
   - `side`: the side of the neighbor, `Left | Right | Top | Bottom`.
   - `offsetInScreens`: the shift along the shared edge. Left/Right is measured from the neighbor's bottom edge in screen heights; Top/Bottom from its left edge in screen widths. 0 lines up the bottom/left edges.
   - `newSceneName`: optional. Defaults to the next `Cave-N`.
3. Call the action in `runBeziActions` and log the returned summary (scene path, origin, bounds, spawn ID).
4. Verify the result:
   - Inspect the new scene with `getSceneHierarchy`. Expect `InitRoom`, the room, the camera, `Load`, and `LevelEntry left` with a `SpawnPoint` child.
   - Optionally capture `getSceneViewImage` focused on the new room to check that it sits flush against the neighbor.
5. Tell the user what is left to do by hand: resizing, moving the LevelEntry, adding more LevelEntries or other adjacent rooms, and adding room content inside `Load`.

To give existing spawn points IDs, use the custom action `RoomBeziActions_GenerateMissingSpawnPointIds({ sceneName })` instead of asking the user to click the inspector button.

## Troubleshooting
| Error | Cause | Solution |
|---|---|---|
| `A scene already exists at ...` | Name collision | Choose another `newSceneName` or leave it empty |
| `No root object tagged 'Room'...` | Neighbor scene is not a standard room | Ask the user which scene holds the room collider |
| `Room generation is not available in Play Mode` | Editor is playing | Ask the user to exit Play Mode |
| Action not found | Editor scripts not compiled yet, or the Bezi package is missing (the `Rooms.BeziActions` assembly only compiles when `com.bezi.sidekick` is installed) | Ask the user to focus Unity, then recompile |
| `Could not find 'RoomGenerator...'` or `MissingMethodException` | The reflection target in `RoomGenerator.cs` was renamed or failed to compile | Check `readConsole`, and restore `GenerateAlignedRoomFromText` / `GenerateMissingSpawnPointIds` |
