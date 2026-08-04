using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave33RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _floorBroken;
    [SerializeField] private GameEventId _hasShadowJump;
    [SerializeField] private GameEventId _afterShadowJumpConversationCompleted;
    [SerializeField] private GameObject _deesPathLeft;
    [SerializeField] private GameObject _deesPathRight;
    [SerializeField] private GameObject[] _rootPlatforms;
    [SerializeField] private ConversationManager _afterShadowJumpConversation;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameObject _eliBlockingFloor;
    [SerializeField] private GameObject _deeBlockingFloor;
    [SerializeField] private GameObject _deeBreakableFloor;
    [SerializeField] private EventReference _blockingFloorSfx;
    [SerializeField] private GameObject _blockDeePathBack;

    private Coroutine _cutsceneCoroutine;
    private EventInstance _blockingFloorSfxInstance;


    void Start() {
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id == CaveTimelineId.Id.Eli)
            _deesPathLeft.SetActive(true);
        else if(id == CaveTimelineId.Id.Both)
            _deesPathRight.SetActive(false);
        else if(id == CaveTimelineId.Id.Dee) {
            _blockDeePathBack.SetActive(true);
            CaveAvatar.obj.gameObject.SetActive(true);
            CaveAvatar.obj.SetStartingPositionInCaveRoom33(); 
        }

        if(id == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_hasShadowJump)) {
            _deeBlockingFloor.SetActive(true);
            _deeBreakableFloor.SetActive(false);
        }

        if(GameManager.obj.HasEvent(_hasShadowJump) && !GameManager.obj.HasEvent(_afterShadowJumpConversationCompleted)) {
            _afterShadowJumpConversation.enabled = true;
            _afterShadowJumpConversation.OnConversationEnd += OnAfterShadowJumpConversationCompleted;
            foreach(GameObject platform in _rootPlatforms)
                platform.SetActive(true);
        }

        if(GameManager.obj.HasEvent(_afterShadowJumpConversationCompleted))
            _eliBlockingFloor.SetActive(true);
    }

    void OnDestroy()
    {
        _afterShadowJumpConversation.OnConversationEnd -= OnAfterShadowJumpConversationCompleted;
    }

    public void OnEliRoomEnter() {
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id == CaveTimelineId.Id.Eli && !GameManager.obj.HasEvent(_hasShadowJump)) {
            MusicManager.obj.Stop();
            AmbienceManager.obj.Play(_caveMain);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }

    public void OnFloorBroken() {
        GameManager.obj.RegisterEvent(_floorBroken);
    }

    public void OnReturnFromShadowJumpRooms() {
        if(!GameManager.obj.HasEvent(_hasShadowJump) || GameManager.obj.HasEvent(_afterShadowJumpConversationCompleted))
            return;
        
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.SetFlipX(true);
        _cutsceneCoroutine = StartCoroutine(StartConversation());
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);
        _afterShadowJumpConversation.HardStopConversation();
        _afterShadowJumpConversation.CleanUp();
        _afterShadowJumpConversation.OnConversationEnd -= OnAfterShadowJumpConversationCompleted;
        _afterShadowJumpConversation.enabled = false;

        CaveAvatar.obj.IsFollowingPlayer = true;

        SpriteRenderer spriteRenderer = _eliBlockingFloor.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        _eliBlockingFloor.SetActive(true);

        AudioUtils.SafeStop(ref _blockingFloorSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        GameManager.obj.RegisterEvent(_afterShadowJumpConversationCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator StartConversation() {
        PauseMenuManager.obj.RegisterSkippable(this);
        _blockingFloorSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_blockingFloorSfx, _eliBlockingFloor.gameObject);
        _blockingFloorSfxInstance.start();
        _blockingFloorSfxInstance.release();
        StartCoroutine(FadeInBlockingFloor(_eliBlockingFloor));
        yield return new WaitForSeconds(1.5f);
        _afterShadowJumpConversation.StartConversation();
    }

    private IEnumerator FadeInBlockingFloor(GameObject blockingFloor) {
        SpriteRenderer spriteRenderer = blockingFloor.GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        blockingFloor.SetActive(true);
        while(spriteRenderer.color.a < 1) {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.b, spriteRenderer.color.g, Mathf.MoveTowards(spriteRenderer.color.a, 1, 1.5f * Time.deltaTime));
            yield return null;
        }
    }

    private void OnAfterShadowJumpConversationCompleted() {
        _afterShadowJumpConversation.CleanUp();
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.IsFollowingPlayer = true;
        GameManager.obj.RegisterEvent(_afterShadowJumpConversationCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _afterShadowJumpConversation.OnConversationEnd -= OnAfterShadowJumpConversationCompleted;
        _afterShadowJumpConversation.enabled = false;
        PauseMenuManager.obj.UnregisterSkippable();
    }
}
