using System.Collections;
using UnityEngine;

public class FirstCaveCollectibleConversationTrigger : MonoBehaviour
{
    [SerializeField] private Transform _caveAvatarTarget;
    [SerializeField] private ConversationManager _conversationManager;
    private bool _isTriggered = false;

    void Start()
    {
        if(GameEventManager.obj.FirstCaveCollectibleConversationEnded)
            gameObject.SetActive(false);
        else
            _conversationManager.OnConversationEnd += OnConversationCompleted;
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
        CaveAvatar.obj.FollowPlayer();
        GameEventManager.obj.FirstCaveCollectibleConversationEnded = true;
    }
}
