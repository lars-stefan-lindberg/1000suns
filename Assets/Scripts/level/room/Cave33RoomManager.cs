using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave33RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _floorBroken;
    [SerializeField] private GameEventId _hasShadowJump;
    [SerializeField] private GameEventId _hasShadowLash;
    [SerializeField] private GameEventId _afterShadowJumpConversationCompleted;
    [SerializeField] private GameEventId _afterShadowLashCompleted;
    [SerializeField] private GameEventId _deeCutsceneCompleted;
    [SerializeField] private GameEventId _floorBrokenDee;
    [SerializeField] private GameObject _deesPathLeft;
    [SerializeField] private GameObject _deesPathRight;
    [SerializeField] private GameObject[] _rootPlatforms;
    [SerializeField] private ConversationManager _afterShadowJumpConversation;
    [SerializeField] private ConversationManager _deeCutsceneConversation;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameObject _eliBlockingFloor;
    [SerializeField] private GameObject _deeBlockingFloor;
    [SerializeField] private GameObject _deeBreakableFloor;
    [SerializeField] private GameObject _deeFloatingPlatform;
    [SerializeField] private EventReference _blockingFloorSfx;
    [SerializeField] private GameObject _blockDeePathBack;

    private Coroutine _cutsceneCoroutine;
    private EventInstance _blockingFloorSfxInstance;


    void Start() {
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id == CaveTimelineId.Id.Eli) {
            _deeFloatingPlatform.SetActive(false);
            _deesPathLeft.SetActive(true);

            if(GameManager.obj.HasEvent(_hasShadowJump)) {
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
        else if(id == CaveTimelineId.Id.Both) {
            //TODO
            _deesPathRight.SetActive(false);
            _deesPathLeft.SetActive(false);
        }
        else if(id == CaveTimelineId.Id.Dee) {
            _blockDeePathBack.SetActive(true);
            CaveAvatar.obj.gameObject.SetActive(true);
            CaveAvatar.obj.SetStartingPositionInCaveRoom33(); 

            if(GameManager.obj.HasEvent(_floorBrokenDee))
                _deeBreakableFloor.SetActive(false);
            if(!GameManager.obj.HasEvent(_afterShadowLashCompleted))
                _deeFloatingPlatform.SetActive(false);

            if(!GameManager.obj.HasEvent(_hasShadowLash) && !GameManager.obj.HasEvent(_afterShadowLashCompleted) && !GameManager.obj.HasEvent(_floorBrokenDee)) {
                _deeBreakableFloor.GetComponentInChildren<BreakableFloor>().unbreakable = false;
            } else if(GameManager.obj.HasEvent(_afterShadowLashCompleted)) {
                _deeBreakableFloor.SetActive(false);
                _deeBlockingFloor.SetActive(true);
                _deesPathRight.SetActive(false);
            }
        }
    }

    public void SetDeeFloorBroken() {
        GameManager.obj.RegisterEvent(_floorBrokenDee);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        _afterShadowJumpConversation.OnConversationEnd -= OnAfterShadowJumpConversationCompleted;
        _deeCutsceneConversation.OnConversationEnd -= OnDeeCutsceneCompleted;
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
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void OnReturnFromShadowJumpRooms() {
        if(!GameManager.obj.HasEvent(_hasShadowJump) || GameManager.obj.HasEvent(_afterShadowJumpConversationCompleted))
            return;
        
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.SetFlipX(true);
        _cutsceneCoroutine = StartCoroutine(StartConversation());
    }

    public void OnReturnFromShadowLashRooms() {
        Debug.Log("here");
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id == CaveTimelineId.Id.Eli)
            return;
        Debug.Log("here fff");
        if(!GameManager.obj.HasEvent(_hasShadowLash) || GameManager.obj.HasEvent(_afterShadowLashCompleted))
            return;

        Debug.Log("here awega");
        
        ShadowTwinMovement.obj.Freeze();
        StartCoroutine(AfterShadowLashRoomsCoroutine());
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);

        PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();

        if(playerType == PlayerManager.PlayerType.HUMAN) {
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

            StartCoroutine(ResumeGameplayEli());
        } else if(playerType == PlayerManager.PlayerType.SHADOW_TWIN) {
            _deeCutsceneConversation.HardStopConversation();
            _deeCutsceneConversation.CleanUp();
            _deeCutsceneConversation.OnConversationEnd -= OnDeeCutsceneCompleted;
            _deeCutsceneConversation.enabled = false;

            GameManager.obj.RegisterEvent(_deeCutsceneCompleted);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

            StartCoroutine(ResumeGameplayDee());
        }
    }

    private IEnumerator ResumeGameplayEli() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator ResumeGameplayDee() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
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

    private IEnumerator AfterShadowLashRoomsCoroutine() {
        _deeFloatingPlatform.SetActive(true);
        _blockingFloorSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_blockingFloorSfx, _deeBlockingFloor.gameObject);
        _blockingFloorSfxInstance.start();
        _blockingFloorSfxInstance.release();
        StartCoroutine(FadeInBlockingFloor(_deeBlockingFloor));
        yield return new WaitForSeconds(1.5f);
        _deesPathRight.GetComponent<FadeOutTilemap>().Reveal();
        yield return new WaitForSeconds(1.5f);
        GameManager.obj.RegisterEvent(_afterShadowLashCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        ShadowTwinMovement.obj.UnFreeze();
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

    private void OnDeeCutsceneCompleted() {
        PauseMenuManager.obj.UnregisterSkippable();
        _deeCutsceneConversation.CleanUp();
        _deeCutsceneConversation.OnConversationEnd -= OnDeeCutsceneCompleted;
        _deeCutsceneConversation.enabled = false;
        PauseMenuManager.obj.UnregisterSkippable();
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_deeCutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void DeeCutscene() {
        PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
        if(playerType != PlayerManager.PlayerType.SHADOW_TWIN) {
            return;
        }
        if(GameManager.obj.HasEvent(_deeCutsceneCompleted)) {
            return;
        }
        ShadowTwinMovement.obj.Freeze();
        PauseMenuManager.obj.RegisterSkippable(this);
        _cutsceneCoroutine = StartCoroutine(PlayDeeCutscene());
    }

    private IEnumerator PlayDeeCutscene() {
        yield return new WaitForSeconds(1f);
        if(!ShadowTwinMovement.obj.IsFacingLeft()) {
            ShadowTwinMovement.obj.FlipPlayer();
        }
        CaveAvatar.obj.SetFlipX(false);
        yield return new WaitForSeconds(0.5f);
        _deeCutsceneConversation.enabled = true;
        _deeCutsceneConversation.OnConversationEnd += OnDeeCutsceneCompleted;
        _deeCutsceneConversation.StartConversation();

        yield return null;
    }
}
