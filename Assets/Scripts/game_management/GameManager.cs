using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager obj;
    public GameProgress Progress { get; private set; }
    public bool isDevMode = false;
    private CaveTimeline _caveTimeline;
    private string _currentSpawnPointId;

    [Header("Debug (Read Only)")]
    [SerializeField]
    GameProgressDebugger debugView;

    public bool IsPauseAllowed { get; set; }

    public CaveTimelineId.Id startingTimeline = CaveTimelineId.Id.Eli;

    void Awake() {
        obj = this;
        Progress = new GameProgress();
        // var testEvent = ScriptableObject.CreateInstance<GameEventId>();
        // testEvent.id = "cave-3.soot-freed";
        // Progress.RegisterEvent(testEvent);
        _caveTimeline = new CaveTimeline(startingTimeline);

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

    public bool HasEvent(string eventIdString)
    {
        return Progress.HasEvent(eventIdString);
    }

    public void RegisterEvent(GameEventId eventId)
    {
        Progress.RegisterEvent(eventId);
    }

    public void NewGame()
    {
        Progress.Clear();
    }

    public void SetCurrentSpawnPointId(string spawnPointId)
    {
        _currentSpawnPointId = spawnPointId;
    }

    public string GetCurrentSpawnPointId()
    {
        return _currentSpawnPointId;
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
