using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeforePowerUpRoomsConversationTrigger : MonoBehaviour, ISkippable
{
    [SerializeField] private Transform _caveAvatarTarget;
    [SerializeField] private Transform _caveAvatarAfterConversationTarget;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameEventId _cave33FirstEliSootConversationCompleted;
    private bool _isTriggered = false;
    private Coroutine _cutsceneCoroutine;

    void Start()
    {
        if(GameManager.obj.HasEvent(_cave33FirstEliSootConversationCompleted))
            gameObject.SetActive(false);
        else {
            _conversationManager.enabled = true;
            _conversationManager.OnConversationEnd += OnConversationCompleted;
        }
    }

    void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(_isTriggered)
            return;
        if(other.gameObject.CompareTag("Player")) {
            _isTriggered = true;
            _cutsceneCoroutine = StartCoroutine(StartCutscene());
        }
    }

    private IEnumerator StartCutscene() {
        PlayerMovement.obj.Freeze();

        PauseMenuManager.obj.RegisterSkippable(this);

        yield return new WaitForSeconds(1);

        CaveAvatar.obj.SetTarget(_caveAvatarTarget);
        
        yield return new WaitForSeconds(3);

        _conversationManager.StartConversation();
    }

    public void OnConversationCompleted() {
        _conversationManager.CleanUp();
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.SetTarget(_caveAvatarAfterConversationTarget);
        GameManager.obj.RegisterEvent(_cave33FirstEliSootConversationCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.enabled = false;
        PauseMenuManager.obj.UnregisterSkippable();
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);
        
        _conversationManager.HardStopConversation();
        _conversationManager.CleanUp();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.enabled = false;

        CaveAvatar.obj.IsFollowingPlayer = false;
        CaveAvatar.obj.SetPosition(_caveAvatarAfterConversationTarget.position);
        
        GameManager.obj.RegisterEvent(_cave33FirstEliSootConversationCompleted);
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
}
