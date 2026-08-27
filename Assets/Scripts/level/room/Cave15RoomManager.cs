using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class Cave15RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameObject _eliBreakableWall;
    [SerializeField] private GameObject _eliBlockingWall;
    [SerializeField] private GameObject _deeBlockingTiles;
    [SerializeField] private GameEventId _eliBrokeWall;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameObject _cutsceneCamera;
    [SerializeField] private Transform _deeCutsceneEliStartPosition;
    [SerializeField] private Transform _deeCutsceneEliEndPosition;

    private CaveTimelineId.Id _caveTimelineId;
    private Coroutine _cutsceneCoroutine;

    void Start()
    {
        _caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(_caveTimelineId == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_eliBrokeWall)) {
                _eliBreakableWall.SetActive(false);
                _eliBlockingWall.SetActive(false);
            }
        } else if(_caveTimelineId == CaveTimelineId.Id.Dee) {
            _deeBlockingTiles.SetActive(true);
        } else if(_caveTimelineId == CaveTimelineId.Id.Both) {
            _eliBreakableWall.SetActive(false);
            _eliBlockingWall.SetActive(false);
        }
    }

    public void OnEliBrokeWall() {
        if(_caveTimelineId == CaveTimelineId.Id.Eli) {
            _eliBlockingWall.SetActive(false);
            GameManager.obj.RegisterEvent(_eliBrokeWall);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        } else if(_caveTimelineId == CaveTimelineId.Id.Dee) {
            var tilemap = _deeBlockingTiles.GetComponentInChildren<Tilemap>();
            DOTween.To(() => tilemap.color.a, x => tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, x), 0, 1);
        }
    }

    public void StartDeeCutscene() {
        if(_caveTimelineId != CaveTimelineId.Id.Dee) {
            return;
        }
        ShadowTwinMovement.obj.Freeze();
        _cutsceneCoroutine = StartCoroutine(StartDeeCutsceneCoroutine());
    }

    private IEnumerator StartDeeCutsceneCoroutine() {
        PauseMenuManager.obj.RegisterSkippable(this);

        _conversationManager.OnConversationEnd += OnConversationCompleted;

        //Setup Eli position
        Player.obj.gameObject.SetActive(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        Player.obj.transform.position = _deeCutsceneEliStartPosition.position;

        //Set cave avatar start position, and follow Eli
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.OverriddenPlayerType = PlayerManager.PlayerType.HUMAN;
        CaveAvatar.obj.SetFollowPlayerStartingPosition();
        CaveAvatar.obj.IsFollowingPlayer = true;

        yield return new WaitForSeconds(1f);
        if(!ShadowTwinMovement.obj.IsFacingLeft()) {
            ShadowTwinMovement.obj.FlipPlayer();
        }

        yield return new WaitForSeconds(1f);

        //Break wall programmatically
        _eliBreakableWall.GetComponentInChildren<BreakableWall>().breakWall = true;
        _eliBlockingWall.SetActive(false);

        yield return new WaitForSeconds(2f);

        //Steer Eli to designated position
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(0.8f);
        PlayerMovement.obj.SimulateJumpInput(true, Time.time);
        yield return null;
        yield return new WaitForSeconds(1.3f);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);

        _cutsceneCamera.SetActive(true);

        yield return new WaitForSeconds(2f);

        _conversationManager.StartConversation();
    }

    public void RequestSkip() {
        _cutsceneCamera.SetActive(false);
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }

        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.CleanUp();

        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        Player.obj.transform.position = _deeCutsceneEliEndPosition.position;
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();

        //Set position of cave avatar and follow Eli
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.SetFollowPlayerStartingPosition();
        CaveAvatar.obj.OverriddenPlayerType = PlayerManager.PlayerType.HUMAN;
        CaveAvatar.obj.IsFollowingPlayer = true;
        
        _eliBreakableWall.SetActive(false);
        _eliBlockingWall.SetActive(false);
        _deeBlockingTiles.SetActive(true);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);

        //Start Dee movement into next room
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);

        //Rely on trigger in next room to enable pause, set Eli and Soot inactive, and save

        yield return null;
    }

    private void OnConversationCompleted() {
        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() { 
        yield return new WaitForSeconds(1f);

        //Start Dee movement into next room
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);

        _cutsceneCamera.SetActive(false);
        _conversationManager.CleanUp();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        
        yield return null;
    }

    private void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
