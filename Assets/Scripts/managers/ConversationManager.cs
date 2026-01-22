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
        if (conversationList.Count > 0)
        {
            currentDialogueIndex = 0;
            SoundFXManager.obj.PlayDialogueOpen();
            ShowNextDialogue();
        }
    }

    public void HardStopConversation() {
        currentDialogueIndex = conversationList.Count;
        if(_dialogueController.IsDisplayed()) {
            _dialogueController.HardStopConversation();
        }
    }

    private void ShowNextDialogue()
    {
        if (currentDialogueIndex < conversationList.Count)
        {
            ConversationEntry entry = conversationList[currentDialogueIndex];
            _dialogueController.ShowDialogue(entry.dialogueContent);
        }
        else
        {
            EndConversation();
        }
    }

    private void OnDialogueClosing() {
        currentDialogueIndex++;
        if (currentDialogueIndex == conversationList.Count)
            SoundFXManager.obj.PlayDialogueClose();
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
        OnConversationEnd?.Invoke();
    }
}
