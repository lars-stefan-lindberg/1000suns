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
        _dialogueController.gameObject.SetActive(true);
        if (conversationList.Count > 0)
        {
            currentDialogueIndex = 0;
            ShowNextDialogue();
        }
    }

    public void HardStopConversation() {
        currentDialogueIndex = conversationList.Count;
        if(_dialogueController.IsDisplayed()) {
            _dialogueController.HardStopConversation();
        }
        _dialogueController.CleanUp();
        _dialogueController.gameObject.SetActive(false);
    }

    public void CleanUp() {
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
        if(_isLastConversationOfRoom)
            _dialogueController.gameObject.SetActive(false);
        OnConversationEnd?.Invoke();
    }
}
