using System.Collections;
using UnityEngine;

public class C1DialogueTrigger : MonoBehaviour, ISkippable
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private CaveAvatarRootsManager _caveAvatarRootsManager;
    [SerializeField] private GameObject _caveRootsTrap;
    [SerializeField] private GameObject _nextDialogueTrigger;
    [SerializeField] private Transform _finalCaveAvatarFlyPosition;
    private BoxCollider2D _collider;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
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
        _caveAvatarRootsManager.Stop();
        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        
        _caveRootsTrap.SetActive(false);
        _nextDialogueTrigger.SetActive(false);
        _caveAvatarRootsManager.gameObject.SetActive(false);

        CaveAvatar.obj.SetFloatingEnabled(true);
        CaveAvatar.obj.SetPosition(_finalCaveAvatarFlyPosition.position);
        CaveAvatar.obj.SetFlipX(false);
        Player.obj.transform.position = new Vector2(273f, -90.875f);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
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
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _nextConversationManager.enabled = true;
    }
}
