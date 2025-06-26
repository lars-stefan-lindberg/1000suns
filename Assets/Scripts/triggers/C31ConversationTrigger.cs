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
        yield return new WaitForSeconds(1f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() {
        MusicManager.obj.PlayCaveAvatarChase();
        yield return new WaitForSeconds(3f);
        _c31Manager.StartAttackSequence();
        yield return new WaitForSeconds(1f);
        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.C31CutsceneCompleted = true;
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        yield return null;
    }
}
