# Rooms

How rooms are built in 1000 Suns, and how to generate new ones. This is the reference for anyone, human or agent, who creates or edits room scenes.

## Basics

- **One room = one Unity scene** in `Assets/Scenes/` (for example `Cave-60.unity`). New rooms are named `Cave-N`.
- **All rooms share one world space.** Neighboring rooms sit flush against each other in world coordinates, with no gap or overlap. For example, Cave-60 is at (2380, -344) and Cave-61 is at (2460, -344).
- **Pixel grid:** 8 pixels per unit. One screen is 320x180 px, which is **40 x 22.5 units**.
- **Everything is snapped to the pixel grid**, meaning positions are multiples of 0.125 units. This matters most for containers such as `Load`: if a parent is off the grid, every child ends up off the grid too.
- **Room types are biome-agnostic.** The prefabs are called `Cave...Room` for historical reasons, but every biome (Cave, Forest, ...) uses them.
- **Biome scenes:** each room refers to one background scene (`CaveBg1`, `CaveBg2`, `ForestBg1`, ...) and one walkable-surfaces scene (`Cave-surfaces-1` to `Cave-surfaces-6`, `Forest-surfaces`, ...).

## The four standard room types

All prefabs are in `Assets/Prefabs/level/scene_template/`.

| Type | Room prefab | Camera prefab | `RoomCameraController.roomType` | Size (screens) | Size (units) | Where the room origin sits | Local collider rect (x, y) |
|---|---|---|---|---|---|---|---|
| Standard | `CaveStandardRoom` | `MainVCam static` | Static | 1 x 1 | 40 x 22.5 | center of the room | -20..20, -11.25..11.25 |
| Horizontal | `CaveHorizontalRoom` | `MainVCam horizontal` | Horizontal | 2 x 1 | 80 x 22.5 | center of the **left** screen | -20..60, -11.25..11.25 |
| Vertical | `CaveVerticalRoom` | `MainVCam vertical` | Vertical | 1 x 3 | 40 x 67.5 | center of the **top** screen | -20..20, -56.25..11.25 |
| Big | `CaveBigRoom` | `MainVCam big room` | HorizontalAndVertical | 1.5 x 1.5 | 60 x 33.75 | center of the **bottom-left** screen | -20..40, -11.25..22.5 |

- These are the *standard* sizes. Individual rooms are often made bigger or smaller afterwards by editing the room's `PolygonCollider2D` points. Always read the real size from the collider in the scene, not from this table.
- The colliders have about 0.01 units of padding (e.g. `20.01`, `11.26`). Snap them to the pixel grid before doing any alignment math.

## What every room scene contains

These are the root objects, in the order they usually appear:

```
InitRoom              # Room configuration (tag: InitRoom)
Cave<Type>Room        # Room bounds + RoomMgr (tag: Room)
MainVCam <type>       # Cinemachine room camera (tag: MainCamera)
Load                  # Container for everything that is loaded/unloaded with the room
LevelEntry left       # Player trigger that makes this the active room
└── SpawnPoint        # Spawn point with generated ID (tag: PlayerSpawnPoint)
```

### InitRoom (`InitRoom.prefab`, script `Assets/Scripts/level/InitRoom.cs`)
- `backgroundScene` (`SceneField`): the background scene to load.
- `walkableSurfaceScene` (`SceneField`): the walkable-surfaces scene. It is added in `Start` through `WalkableSurfacesManager`.
- `adjacentRooms` (`List<SceneField>`): the rooms loaded automatically when this room is loaded, or when a `LevelEntry` in this room is entered. Any other loaded room that is not on this list gets unloaded (`LevelManager.LoadAdjacentRooms` / `UnloadNonAdjacentRooms`). The list should be symmetric: if A lists B, B lists A.
- `defaultSpawnPoint` (`GameObject`): normally the `SpawnPoint` child of the room's first `LevelEntry`.
- `isRetryable` (`bool`): whether the room can be retried. If the same GameObject has an `IRetryable` component, that component decides instead (e.g. `Cave23RetryRoomManager`).
- A `SceneField` serializes two values, `_sceneAsset` (a `SceneAsset`) and `_sceneName` (a string). When setting one from code, set both.

### Room (`Cave<Type>Room.prefab`, tag `Room`, layer `Ignore Raycast`)
- `PolygonCollider2D` (trigger): the room bounds. At runtime it also becomes the camera confiner. `LevelEntry` passes it to `CameraManager.obj.EnterRoom(...)`, which is why the camera's `CinemachineConfiner2D.m_BoundingShape2D` is left empty in the scene.
- `RoomMgr` (`Assets/Scripts/managers/RoomMgr.cs`):
  - `roomObjectsToLoad`: must reference the scene's `Load` object.
  - `_darknessLevelType`: the room's darkness level, set through the `DarknessLevel` component on the same object.
  - `_disableUnloadRoomObjects`, `_isSecretRoom`.
  - `OnRoomEnter` / `OnRoomExit` events.
  - `CustomCameraHandling`: if this event has a listener, `RoomMgr` does not turn on the main camera itself.

### Camera (`MainVCam <type>.prefab`, tag `MainCamera`)
- `CinemachineVirtualCamera` (orthographic size 11.25), `CinemachinePixelPerfect`, `CinemachineConfiner2D`, `RoomCameraController`, plus a `cm` child with a `CinemachineFramingTransposer`.
- Positioned at the room origin, keeping the prefab's z (-1.15).
- `RoomCameraController.roomType` must match the room type (see the table). Vertical rooms also have top-lock settings.
- `MainVCam.prefab` (without a suffix) is legacy and should not be used for new rooms.

### Load (`Load.prefab`)
- An empty container. Put all of the room's gameplay content (platforms, enemies, props that load and unload) inside it.
- `RoomMgr.roomObjectsToLoad` must reference it.
- It is inactive in saved scenes, and `RoomMgr` toggles it.
- Its position must be on the pixel grid. The generator places it at the room origin.

### LevelEntry (`LevelEntry left|right|top|bottom.prefab`, tag `LevelEntry`, script `Assets/Scripts/level/LevelEntry.cs`)
- `BoxCollider2D` trigger, 1 x 3.75 units for left/right. It sits just inside the room edge where the player enters.
- When the player enters it, it:
  - makes this scene the active scene
  - loads the adjacent rooms, then unloads non-adjacent rooms after the camera blend finishes
  - activates this room's camera (or fires `OnCustomCameraHandling`)
  - stores the spawn point ID
  - optionally saves the game.
- Options: `_fireCustomCameraHandlingEvent`, `_enablePlayerTransition`, `_shouldSaveGame`, `_unduckMusic`.
- The `SpawnPoint` child (`Assets/Scripts/level/SpawnPoint.cs`) needs a unique ID in the form `<SceneName>_<GUID>`. You can generate it with:
  - the **Generate Spawn point ID** button in the inspector
  - the Bezi action `RoomBeziActions_GenerateMissingSpawnPointIds({ sceneName })`.
- You can check IDs with `Tools > Spawn Points > Validate Duplicate IDs / Find Missing IDs`.
- A room can have several LevelEntries, one per entrance.

### Optional extras (per room, added by hand)
- `RoomManager`: a GameObject with a room-specific script (`Assets/Scripts/level/room/CaveXXRoomManager.cs`). These scripts return early from `Start` when the scene is loaded as an adjacent room.
- `KillTrigger`, `PlayerTrigger`, extra `SpawnPoint`s, background layers, and so on.

## Legacy you will run into
- **`.scenetemplate` files** (`Assets/Scenes/Cave*Room.scenetemplate`) came from an abandoned experiment. Don't use them.
- **Per-scene prefab copies** in folders like `Assets/Scenes/Cave-14/` (`CaveStandardRoom.prefab`, `InitRoom.prefab`, `LevelEntry.prefab`, ...) were cloned by that experiment. Older rooms still use them. New rooms always use the shared prefabs in `Assets/Prefabs/level/scene_template/`.

## Generating a new room

The generator does exactly what you would do by hand: it drags the standard prefabs into a new scene. It also places the room and wires everything up. Code: `Assets/Editor/Rooms/RoomGenerator.cs`.

**From the editor:** `Tools > Rooms > Generate Aligned Room...`. The currently active scene is pre-filled as the room to align to.

**From a Bezi agent** (only when the Bezi plugin is installed):
- The actions are in `Assets/Editor/Rooms/BeziActions/`, in their own assembly, `Rooms.BeziActions`. Unity only compiles that assembly when the `com.bezi.sidekick` package is present, so the project compiles fine without Bezi.
- The package folder is gitignored, and every machine installs it through the Bezi app.
- That assembly calls `RoomGenerator.GenerateAlignedRoomFromText` and `RoomGenerator.GenerateMissingSpawnPointIds` by reflection, so don't rename them or change their signatures.
- `RoomBeziActions_GenerateAlignedRoom({ roomType, neighborScene, side, offsetInScreens, newSceneName })`, which returns `{ result: string }`.
- `roomType`: `Standard | Horizontal | Vertical | Big`.
- `neighborScene`: an existing scene name (`"Cave-60"`) or an asset path.
- `side`: `Left | Right | Top | Bottom`, i.e. the side of the existing room.
- `offsetInScreens`: how far to shift the new room along the shared edge.
  - For Left/Right it is measured from the neighbor's bottom edge, where 1 = 22.5 units.
  - For Top/Bottom it is measured from the neighbor's left edge, where 1 = 40 units.
  - `0` lines up the bottom (or left) edges. Fractions and negative values are allowed.
- `newSceneName`: optional. Leave it empty to use the next free `Cave-N`.

**What it does:**
1. Opens the neighbor scene additively and reads its world-space room bounds from the `Room` collider.
2. Creates `Assets/Scenes/<name>.unity` and adds it to Build Settings.
3. Instantiates `InitRoom`, the room prefab, the matching camera, `Load`, and `LevelEntry left` as prefab instances. Everything is snapped to the pixel grid, and the room sits flush against the requested side.
4. Places `LevelEntry left` just inside the new room's left edge, at the height of the room origin.
5. Generates the spawn point ID and sets `InitRoom.defaultSpawnPoint` to it.
6. Wires up `RoomMgr.roomObjectsToLoad = [Load]`.
7. Copies `backgroundScene`, `walkableSurfaceScene`, `isRetryable`, and the darkness level from the neighbor.
8. Adds each room to the other's `adjacentRooms`.
9. Saves both scenes and leaves the new scene open next to its neighbor.

**What it does not do (left to you):**
- Change the room size from the standard size.
- Move the LevelEntry or add more LevelEntries.
- Set up adjacency with rooms other than the neighbor.
- Add room content or a `RoomManager` script.
