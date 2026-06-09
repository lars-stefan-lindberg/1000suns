using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class C35OverhearConversationTrigger : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    private BoxCollider2D _boxCollider2D;

    void Awake() {
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed += OnDialogueCompleted;
        }    
    }

    void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _boxCollider2D.enabled = false;
            StartCoroutine(HandleDialogue());
        }
    }

    private void OnDialogueCompleted() {
        PlayerMovement.obj.UnFreeze();
        _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
        _nextConversationManager.enabled = true;
        //Reset text size
        _dialogueText.fontSize += 10;
    }

    private IEnumerator HandleDialogue() {
        if(Player.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze();
        } else if(PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze();
            PlayerBlobMovement.obj.ToHuman();
        }

        //Decrease text size to make it look like you are overhearing
        _dialogueText.fontSize -= 10;
        yield return new WaitForSeconds(2f);
        
        _dialogueController.gameObject.SetActive(true);
        _dialogueController.ShowDialogue(_dialogueContent, true, true);
        
        yield return null;
    }
}
