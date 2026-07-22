using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave33RoomManager : MonoBehaviour
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
        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation() {
        StartCoroutine(FadeInBlockingFloor(_eliBlockingFloor));
        yield return new WaitForSeconds(1.5f);
        _afterShadowJumpConversation.StartConversation();
    }

    private IEnumerator FadeInBlockingFloor(GameObject blockingFloor) {
        SoundFXManager.obj.PlayAtPosition(_blockingFloorSfx, blockingFloor.transform.position);
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
    }
}
