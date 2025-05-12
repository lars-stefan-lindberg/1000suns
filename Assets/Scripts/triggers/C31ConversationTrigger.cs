using System.Collections;
using UnityEngine;

public class C31ConversationTrigger : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private C31Manager _c31Manager;
    private BoxCollider2D _collider;

    void Start() {
        if(GameEventManager.obj.C31CutsceneCompleted) {
            Destroy(this);
        }
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

    private IEnumerator SetupDialogue() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(0.5f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        _c31Manager.StartAttackSequence();
        GameEventManager.obj.C31CutsceneCompleted = true;
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
