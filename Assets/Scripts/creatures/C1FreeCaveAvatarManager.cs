using System.Collections;
using UnityEngine;

public class C1FreeCaveAvatarManager : MonoBehaviour, ISkippable
{
    [SerializeField] private Transform[] _caveAvatarFreePositions;
    [SerializeField] private Transform _finalCaveAvatarFlyPosition;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameObject _caveRootsTrap;
    private BoxCollider2D _collider;
    private Coroutine _cutsceneCoroutine;

    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }
    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            _cutsceneCoroutine = StartCoroutine(StartCutscene());
        }
    }

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        Player.obj.EndPullRoots();
        if(_caveRootsTrap != null)
            _caveRootsTrap.SetActive(false);
        CaveAvatar.obj.SetFloatingEnabled(true);
        CaveAvatar.obj.SetPosition(_finalCaveAvatarFlyPosition.position);
        CaveAvatar.obj.SetFlipX(false);
        _conversationManager.HardStopConversation();
        Player.obj.transform.position = new Vector2(273f, -90.875f);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PauseMenuManager.obj.UnregisterSkippable();
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        yield return null;
    }

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(1f);

        Player.obj.PlayPullRoots();

        yield return new WaitForSeconds(4.8f);

        Player.obj.EndPullRoots();

        //Soot flies happily
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[0]);
        CaveAvatar.obj.SetFloatingEnabled(true);
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[1]);
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[2]);
        yield return new WaitForSeconds(1.5f);

        //Start dialogue
        _conversationManager.StartConversation();

        yield return null;
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.SetTarget(_finalCaveAvatarFlyPosition);
        PauseMenuManager.obj.UnregisterSkippable();
    }
}
