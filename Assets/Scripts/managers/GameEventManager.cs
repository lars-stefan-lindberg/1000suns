using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager obj;

    //Cutscene events
    public bool CaveLevelStarted { get; set; }
    public bool C1MonologueEnded { get; set; }
    public bool CaveAvatarFreed { get; set; }
    public bool CapeRoomZoomCompleted { get; set; }
    public bool CapePicked { get; set; }
    public bool FirstCaveCollectibleConversationEnded { get; set; }
    public bool BabyPrisonerAlerted { get; set; }
    public bool PrisonerIntroSeen { get; set; }
    public bool FirstPrisonerFightStarted { get; set; }
    public bool FirstPrisonerKilled { get; set; }
    public bool FirstCaveMiniBossKilled { get; set; }
    public bool MirrorConversationEnded { get; set; }
    public bool PowerUpRoomsFloorBroken { get; set; }
    public bool FirstPowerUpPicked { get; set; }
    public bool PowerUpRoomCompletedWallBreak { get; set; }
    public bool AfterPowerUpRoomsCompletedWallBreak { get; set; }
    public bool C215WallBroken { get; set; }
    public bool C275FloorBroken { get; set; }
    public bool C26CutsceneCompleted { get; set; }
    public bool C27CutsceneCompleted { get; set; }
    public bool C30CutsceneCompleted { get; set; }
    public bool C31CutsceneCompleted { get; set; }

    public bool IsPauseAllowed { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
    }

    public void ResetGameEvents() {
        CaveAvatarFreed = false;
        PrisonerIntroSeen = false;
        BabyPrisonerAlerted = false;
        CapeRoomZoomCompleted = false;
        CapePicked = false;
        FirstPowerUpPicked = false;
        PowerUpRoomsFloorBroken = false;
        CaveLevelStarted = false;
        AfterPowerUpRoomsCompletedWallBreak = false;
        PowerUpRoomCompletedWallBreak = false;
        IsPauseAllowed = true;
        C215WallBroken = false;
        C275FloorBroken = false;
        FirstCaveCollectibleConversationEnded = false;
        FirstPrisonerFightStarted = false;
        FirstPrisonerKilled = false;
        FirstCaveMiniBossKilled = false;
        C1MonologueEnded = false;
        C26CutsceneCompleted = false;
        C27CutsceneCompleted = false;
        C30CutsceneCompleted = false;
        MirrorConversationEnded = false;
        C31CutsceneCompleted = false;
    }

    void OnDestroy() {
        obj = null;
    }

    // Returns a list of event keys (by property name) that have occurred
    public List<string> GetCompletedEvents()
    {
        var completed = new List<string>();
        var props = typeof(GameEventManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var prop in props)
        {
            // Only persist bool properties that are both readable and writable, excluding IsPauseAllowed
            if (prop.PropertyType == typeof(bool) && prop.CanRead && prop.CanWrite && prop.Name != nameof(IsPauseAllowed))
            {
                var value = (bool)(prop.GetValue(this) ?? false);
                if (value)
                {
                    completed.Add(prop.Name);
                }
            }
        }
        return completed;
    }

    // Applies a list of event keys (by property name) to set occurred events
    public void ApplyCompletedEvents(List<string> events)
    {
        // Start from a clean slate
        ResetGameEvents();

        if (events == null || events.Count == 0)
        {
            return;
        }

        var toSet = new HashSet<string>(events);
        var props = typeof(GameEventManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var prop in props)
        {
            // Only apply to bool properties that are both readable and writable, excluding IsPauseAllowed
            if (prop.PropertyType == typeof(bool) && prop.CanWrite && prop.CanRead && prop.Name != nameof(IsPauseAllowed))
            {
                if (toSet.Contains(prop.Name))
                {
                    prop.SetValue(this, true);
                }
            }
        }
    }
}
