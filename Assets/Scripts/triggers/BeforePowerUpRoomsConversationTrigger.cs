using System.Collections;
using UnityEngine;

public class BeforePowerUpRoomsConversationTrigger : MonoBehaviour
{
    [SerializeField] private Transform _caveAvatarTarget;
    [SerializeField] private Transform _caveAvatarAfterConversationTarget;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameEventId _cave33FirstEliSootConversationCompleted;
    private bool _isTriggered = false;

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
            StartCoroutine(StartCutscene());
        }
    }

    private IEnumerator StartCutscene() {
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(1);

        CaveAvatar.obj.SetTarget(_caveAvatarTarget);
        
        yield return new WaitForSeconds(3);

        _conversationManager.StartConversation();
    }

    public void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.SetTarget(_caveAvatarAfterConversationTarget);
        GameManager.obj.RegisterEvent(_cave33FirstEliSootConversationCompleted);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.enabled = false;
    }
}
