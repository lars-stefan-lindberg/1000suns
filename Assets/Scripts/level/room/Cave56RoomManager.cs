using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave56RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _cutsceneCompleted;
    [SerializeField] private GameEventId _conversationCompleted;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private Transform _eliStartPosition;
    [SerializeField] private Transform _eliEndPosition;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private Transform _sootEndPosition;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _conversationCompletedCoroutine;
    
    void Start()
    {
        var caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_cutsceneCompleted) || caveTimelineId != CaveTimelineId.Id.Dee) {
            Destroy(this);
            return;
        }

        _conversationManager.OnConversationEnd += OnConversationCompletedDee;
        _conversationManager.enabled = true;

        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.IsFollowingPlayer = false;
        CaveAvatar.obj.SetPosition(_sootStartPosition.position, false);
        CaveAvatar.obj.SetRedEyes();
        CaveAvatar.obj.SetFlipX(false);
    }

    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompletedDee;
    }

    public void StartCutscene() {
        if(GameManager.obj.HasEvent(_cutsceneCompleted)) {
            return;
        }

        ShadowTwinMovement.obj.Freeze();
        _cutsceneCoroutine = StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);
        CaveAvatar.obj.SetTarget(_sootEndPosition, 13);
        yield return new WaitForSeconds(2.5f);

        _conversationManager.StartConversation();

        yield return null;
    }

    private void OnConversationCompletedDee() {
        //Setup Eli position
        Player.obj.gameObject.SetActive(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        Player.obj.transform.position = _eliStartPosition.position;

        _conversationCompletedCoroutine = StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() {  
        //Steer Eli to designated position
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        while(Player.obj.transform.position.x < _eliEndPosition.position.x) {
            yield return null;
        }
        PlayerMovement.obj.SetMovementInput(Vector2.zero);

        yield return new WaitForSeconds(2f);

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        GameManager.obj.RegisterEvent(_conversationCompleted);
        
        //Start Dee movement into next room
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);

        _conversationManager.CleanUp();
        _conversationManager.OnConversationEnd -= OnConversationCompletedDee;
    }


    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);

        if(_conversationCompletedCoroutine != null)
            StopCoroutine(_conversationCompletedCoroutine);

        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompletedDee;
        _conversationManager.CleanUp();

        Player.obj.gameObject.SetActive(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        Player.obj.transform.position = _eliEndPosition.position;
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();

        CaveAvatar.obj.SetPosition(_sootEndPosition.position, false);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);

        GameManager.obj.RegisterEvent(_conversationCompleted);

        //Start Dee movement into next room
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);

        //Rely on trigger in next room to enable pause, set Eli and Soot inactive, and save

        yield return null;
    }
}
