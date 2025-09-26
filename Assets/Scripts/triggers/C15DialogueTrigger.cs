using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C15DialogueTrigger : MonoBehaviour
{
    private BoxCollider2D _collider;
    [SerializeField] private ConversationManager _conversationManager;
    
    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }

    private void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.MirrorConversationEnded) {
            return;
        }
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
        GameEventManager.obj.MirrorConversationEnded = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
