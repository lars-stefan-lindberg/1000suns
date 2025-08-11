using System.Collections;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;
    private BoxCollider2D _collider;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed += OnDialogueCompleted;
            _dialogueController.OnDialogueClosing += OnDialogueClosing;
        }
    }

    private void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
            _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.C1MonologueEnded) {
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
        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent);
    }

    private void OnDialogueClosing() {
        SoundFXManager.obj.PlayDialogueClose();
    }
    private void OnDialogueCompleted() {
        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.C1MonologueEnded = true;
    }
}
