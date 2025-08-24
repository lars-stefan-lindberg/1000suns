using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C35OverhearConversationTrigger : MonoBehaviour
{
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private RectTransform _rightPortrait;
    private BoxCollider2D _boxCollider2D;

    void Awake() {
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed += OnDialogueCompleted;
            _dialogueController.OnDialogueClosing += OnDialogueClosing;
        }    
    }

    void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
            _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _boxCollider2D.enabled = false;
            StartCoroutine(HandleDialogue());
        }
    }

    private void OnDialogueClosing() {
        SoundFXManager.obj.PlayDialogueClose();
    }

    private void OnDialogueCompleted() {
        PlayerMovement.obj.UnFreeze();
        _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
        _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        _nextConversationManager.enabled = true;
        //Reset Soot portrait
        _rightPortrait.localScale = new Vector3(1, 1, 1);
    }

    private IEnumerator HandleDialogue() {
        if(Player.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze();
        } else if(PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze();
            PlayerBlobMovement.obj.ToHuman();
        }

        //Show Soot facing right
        _rightPortrait.localScale = new Vector3(-1, 1, 1);
        yield return new WaitForSeconds(2f);
        
        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent);
        
        yield return null;
    }
}
