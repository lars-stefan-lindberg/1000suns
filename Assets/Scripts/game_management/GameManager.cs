using UnityEngine;

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
        _caveTimeline = new CaveTimeline(CaveTimelineId.Id.Eli);

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

    void OnDestroy() {
        obj = null;
    }
}
