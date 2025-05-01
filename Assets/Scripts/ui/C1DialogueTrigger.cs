using System.Collections;
using UnityEngine;

public class C1DialogueTrigger : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private CaveAvatarRootsManager _caveAvatarRootsManager;
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
        if(GameEventManager.obj.CaveAvatarFreed) {
            return;
        }
        if(other.CompareTag("Player")) {
            _collider.enabled = false;
            StartCoroutine(SetupDialogue());
        }
    }

    private IEnumerator SetupDialogue() {
        PlayerMovement.obj.Freeze();
        GameEventManager.obj.IsPauseAllowed = false;
        yield return new WaitForSeconds(0.5f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        _caveAvatarRootsManager.Stop();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _nextConversationManager.enabled = true;
    }
}
