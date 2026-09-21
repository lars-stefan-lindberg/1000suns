using System.Collections.Generic;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public event System.Action OnConversationEnd;
    [System.Serializable]
    private struct ConversationEntry
    {
        public DialogueContent dialogueContent;
    }
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private List<ConversationEntry> conversationList;
    [SerializeField] private bool _isLastConversationOfRoom = true;
    [SerializeField] private bool _duckMusic = true;
    private int currentDialogueIndex = 0;

    void OnEnable()
    {
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

    public void StartConversation()
    {
        if(_duckMusic)
            AudioStateManager.obj.SetDialogue(true);
        _dialogueController.gameObject.SetActive(true);
        if (conversationList.Count > 0)
        {
            currentDialogueIndex = 0;
            ShowNextDialogue();
        }
    }

    public void HardStopConversation() {
        if(_duckMusic)
            AudioStateManager.obj.SetDialogue(false);
        currentDialogueIndex = conversationList.Count;
        if(_dialogueController.IsDisplayed()) {
            _dialogueController.HardStopConversation();
        }
        _dialogueController.CleanUp();
        _dialogueController.gameObject.SetActive(false);
    }

    public void CleanUp() {
        if(_duckMusic)
            AudioStateManager.obj.SetDialogue(false);
        _dialogueController.CleanUp();
        _dialogueController.gameObject.SetActive(false);
    }

    private void ShowNextDialogue()
    {
        if (currentDialogueIndex < conversationList.Count)
        {
            ConversationEntry entry = conversationList[currentDialogueIndex];
            _dialogueController.ShowDialogue(entry.dialogueContent, currentDialogueIndex == 0, currentDialogueIndex == conversationList.Count - 1);
        }
        else
        {
            EndConversation();
        }
    }

    private void OnDialogueClosing() {
        currentDialogueIndex++;
    }

    private void OnDialogueCompleted()
    {
        if (currentDialogueIndex < conversationList.Count)
        {
            ShowNextDialogue();
        }
        else
        {
            EndConversation();
        }
    }

    private void EndConversation()
    {
        if(_isLastConversationOfRoom) {
            if(_duckMusic)
                AudioStateManager.obj.SetDialogue(false);
            _dialogueController.gameObject.SetActive(false);
        }
        OnConversationEnd?.Invoke();
    }
}
