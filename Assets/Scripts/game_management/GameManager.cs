using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    public static GameManager obj;
    public GameProgress Progress { get; private set; }
    private CaveTimeline _caveTimeline;
    public bool isDevMode = false;

    [Header("Debug (Read Only)")]
    [SerializeField]
    GameProgressDebugger debugView;

    //Cutscene events
    public bool CaveLevelStarted { get; set; }
    public bool C1MonologueEnded { get; set; }
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
    public bool C20CutsceneCompleted { get; set; }
    public bool C215WallBroken { get; set; }
    public bool C275FloorBroken { get; set; }
    public bool C26CutsceneCompleted { get; set; }
    public bool C27CutsceneCompleted { get; set; }
    public bool C30CutsceneCompleted { get; set; }
    public bool C31CutsceneCompleted { get; set; }

    public bool IsPauseAllowed { get; set; }

    void Awake() {
        obj = this;
        Progress = new GameProgress();
        // var testEvent = ScriptableObject.CreateInstance<GameEventId>();
        // testEvent.id = "cave-3.soot-freed";
        // Progress.RegisterEvent(testEvent);
        //CaveTimeline = new CaveTimeline();
        //CaveTimeline.SetTimeline(CaveTimelineId.Id.Eli);

        // Hook debugger if present
        if (debugView != null)
            debugView.Bind(Progress);

        IsPauseAllowed = true;
    }

    // --------- Public API ---------

    public bool HasEvent(GameEventId eventId)
    {
        return Progress.HasEvent(eventId);
    }

    public void RegisterEvent(GameEventId eventId)
    {
        Progress.RegisterEvent(eventId);
    }

    public void NewGame()
    {
        Progress.Clear();
    }

    // --------- Save / Load hooks ---------

    public GameProgress GetProgressForSave()
    {
        return Progress;
    }

    public void LoadProgress(GameProgress loaded)
    {
        Progress = loaded;
    }

    public CaveTimeline GetCaveTimeline() {
        return _caveTimeline;
    }

    public void SetCaveTimeline(CaveTimeline caveTimeline) {
        _caveTimeline = caveTimeline;
    }

    // Returns a list of event keys (by property name) that have occurred
    public List<string> GetCompletedEvents()
    {
        var completed = new List<string>();
        var props = typeof(GameManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
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
        Progress.Clear();

        if (events == null || events.Count == 0)
        {
            return;
        }

        var toSet = new HashSet<string>(events);
        var props = typeof(GameManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
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

    void OnDestroy() {
        obj = null;
    }
}
