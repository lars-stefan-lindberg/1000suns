using System.Collections;
using UnityEngine;

public class Cave15CutsceneManager : MonoBehaviour, ISkippable
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameEventId _firstConfrontationCompleted;
    [SerializeField] private MusicTrack _tenseBassTrack;
    [SerializeField] private Transform _deeCutsceneStartingPosition;
    private BoxCollider2D _collider;
    private Coroutine _onConversationCompletedCoroutine;

    void Start() {
        if(GameManager.obj.HasEvent(_firstConfrontationCompleted)) {
            Destroy(this);
        }

        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;

        ShadowTwinMovement.obj.gameObject.tag = "Untagged"; //Hack to avoid player triggers to activate like RoomMgr and LevelEntry
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinPlayer.obj.ResetAnimator();
        ShadowTwinPlayer.obj.StartAnimator();
        ShadowTwinPlayer.obj.transform.position = _deeCutsceneStartingPosition.position;
    }

    private void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

        void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            _collider.enabled = false;
            StartCoroutine(SetupDialogue());
        }
    }

    public void RequestSkip() {
        if(_onConversationCompletedCoroutine != null) {
            StopCoroutine(_onConversationCompletedCoroutine);
        }

        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        MusicManager.obj.Play(_tenseBassTrack);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        ShadowTwinMovement.obj.gameObject.tag = "Player";

        GameManager.obj.RegisterEvent(_firstConfrontationCompleted);

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

    private IEnumerator SetupDialogue() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(0.5f);
        _conversationManager.StartConversation();
        yield return new WaitForSeconds(0.8f);
        MusicManager.obj.Play(_tenseBassTrack);
    }

    private void OnConversationCompleted() {
        _onConversationCompletedCoroutine = StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() {
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(0, 0));
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        ShadowTwinMovement.obj.gameObject.tag = "Player";

        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_firstConfrontationCompleted);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        PauseMenuManager.obj.UnregisterSkippable();
    }
}
