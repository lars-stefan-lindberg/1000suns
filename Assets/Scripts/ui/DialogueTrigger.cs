using System.Collections;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;
    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
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
        _dialogueController.ShowDialogue(_dialogueContent, true);
    }
}
