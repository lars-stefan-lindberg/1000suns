# AGENTS.md

Unity 2022.3.62f2, 2D project. Game scripts live in `Assets/Scripts`. Third-party libraries in use: DOTween, FMOD, Cinemachine, FunkyCode lighting.

## Verification: compile without the Unity editor
The Unity editor is usually open and recompiles once it gets focus. To type-check changed scripts from the terminal, compile them against the existing assemblies:

```bash
cd "/Users/stefanlindberg/1000 suns demo"; U=/Applications/Unity/Hub/Editor/2022.3.62f2/Unity.app/Contents; REFS=""; for f in $U/Managed/UnityEngine/*.dll Library/ScriptAssemblies/Assembly-CSharp.dll Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll Library/ScriptAssemblies/FMODUnity.dll Assets/Plugins/DOTween/DOTween.dll $U/NetStandard/ref/2.1.0/netstandard.dll $U/NetStandard/compat/2.1.0/shims/netfx/*.dll; do REFS="$REFS -r:$f"; done; $U/NetCoreRuntime/dotnet $U/DotNetSdkRoslyn/csc.dll -nologo -noconfig -nostdlib -target:library -out:/tmp/check.dll -nowarn:0436,0649 $REFS <changed .cs files> 2>&1 | grep -v "CS1701\|CS1702"; rm -f /tmp/check.dll
```

## Project conventions and gotchas
- Managers are singletons with `public static X obj`, for example `PlayerManager.obj`, `Reaper.obj`, `GameManager.obj`, `CameraManager.obj`.
- To get the active player's transform: `PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType())`.
- To kill the player: `Reaper.obj.KillPlayerGeneric(playerType)`, or put a `KillTrigger` on a trigger collider. Death reloads the scene.
- Room managers (`Assets/Scripts/level/room/CaveXXRoomManager.cs`) return early from `Start` when they're loaded as an adjacent room (`gameObject.scene != SceneManager.GetActiveScene()`).
- Progress is tracked with game events: `GameManager.obj.HasEvent/RegisterEvent(GameEventId)`. The "Both" timeline uses `HasTemporaryEvent/RegisterTemporaryEvent` instead.
- Cameras: `RoomMgr` turns on the main camera unless its `CustomCameraHandling` UnityEvent has a listener. Custom handlers call `CameraManager.obj.EnterRoom(roomCameraController, roomCollider, player, spawnPos)`.
- `Pullable` shakes `_renderer.transform` and resets its local position when grabbed, so a pullable's SpriteRenderer must be on a child object, not the root that has the Rigidbody2D.
- Physics layers that matter:
  - `Default` (0) holds the players.
  - `Pullable` (11) does NOT collide with `HazardTrigger` (12).
  - `Enemies` (9) collides with both `Default` and `Pullable`.
  - Raycasts also hit triggers (`queriesHitTriggers` is on).

## Cave boss
There are two separate bosses that share art and `CaveBossTentacleChain`.

**Chase boss** (`Assets/Scripts/enemies/CaveBoss.cs`, `CaveBossTrigger.cs`, Cave61–65 room managers)
- A persistent singleton under `PersistenceGameplay`.
- Crawls in bursts with lagging heads, can catch up to the player, and rumbles.

**Fight boss** (scene `Cave-66`): stationary. Main files:
- `Assets/Scripts/enemies/CaveBossFight.cs`
  - State machine: Idle, Entering, Fighting, HitReaction, Retreating.
  - `_stages` (index = hits taken; hits needed = `_stages.Length`). Each stage has guard tentacles and an attack interval.
  - Public API: `Enter()`, `PlaceAtFightPosition()`, `StartFighting(delay)`, `TryTakeHit()`, `IsHurtbox()`, `event Defeated`.
  - Hit reaction: flash material swap on body and heads, DOTween shake of `_visual`.
  - After the last hit it retreats left while shaking, then destroys itself.
- `Assets/Scripts/enemies/CaveBossAttackTentacle.cs`
  - Root at the attack root's X and the target's Y. The target is fixed at launch: `CaveBossOrb.InPlay` if there is one, otherwise the active player.
  - Sequence: poke out straight → telegraph with a wiggle → straighten → lunge straight → hold with a wiggle → retract → despawn.
- `Assets/Scripts/enemies/CaveBossGuardTentacle.cs`: placed in the scene. `Activate()` grows it out straight, then it idles with a wiggle. `Retract()` straightens it, then pulls it back.
- `Assets/Scripts/enemies/CaveBossTentacleChain.cs`
  - Options: `startCollapsed`, `wiggleWeight` / `SetWiggle(weight, fade)`, `SetTip()` / `TipPosition`.
  - Optional `hazardCollider` (EdgeCollider2D that follows the drawn shape).
  - Static registry: `CaveBossTentacleChain.IsHazard(collider)`.
- `Assets/Scripts/environment/cave/CaveBossOrb.cs`
  - Dee's shadow pull grabs it, which puts it "in play". Static `InPlay` and `SetGrabbingEnabled()` allow only one orb in play; the others are made immune.
  - When released it hangs in the air. Eli's projectile launches it a fixed horizontal distance.
  - It resets on touching a tentacle hazard, on going too far right of its origin, or when the boss rejects the hit. A hit on the hurtbox while flying uses the orb up.
  - Kinematic while hanging, with `useFullKinematicContacts`; detection is done with `OnTriggerEnter2D`.
- `Assets/Scripts/level/room/Cave66RoomManager.cs`
  - `StartFight()` is called from a trigger.
  - Handles the intro (freezes the player, enables the disabled boss, switches to the fight camera), respawning mid fight, the end of the fight, and saving.
  - `HandleCamera()` is wired to `RoomMgr.CustomCameraHandling`.
- Prefabs in `Assets/Prefabs/environment/cave/cave_boss/`:
  - `Cave boss fight.prefab`
  - `AttackTentacle.prefab` (its `HazardCollider` is a sibling of `Tentacle`, on layer `Enemies`)
  - `Magical orb.prefab` (the sprite is on the child `Sprite`)
- The boss hurtbox and tentacle hazard colliders must be triggers on a layer that collides with `Pullable`; `Enemies` is the one in use.

### Open items
- Confirm in Play mode that the orb hit on the boss and the tentacle reset of the orb work.
- Possibly disable the fight camera after the boss is defeated.
- Add sound effects (attack, hit, retreat) and music start/stop.
- Optionally fade the orb when it resets instead of teleporting it.
- Consider temporary events for the "Both" timeline.

## Rooms
One room = one scene in `Assets/Scenes/`, and all rooms share one world space (neighboring rooms sit flush against each other, snapped to an 8 PPU pixel grid). There are four standard room types (Standard, Horizontal, Vertical, Big). They use the `Cave*Room` / `MainVCam *` prefabs in `Assets/Prefabs/level/scene_template/` for every biome. Before creating or editing a room, read `Assets/Editor/Rooms/README.md`: it covers the room anatomy, wiring (InitRoom, RoomMgr/Load, LevelEntry/SpawnPoint IDs), legacy setups to avoid, and the room generator (`Tools > Rooms > Generate Aligned Room...`, or the Bezi action `RoomBeziActions_GenerateAlignedRoom`). Bezi is optional: its package (`Packages/com.bezi.sidekick/`) is gitignored, and the Bezi-only code is in its own assembly that only compiles when the package is installed. Never make non-Bezi code depend on the `Bezi` namespace.
