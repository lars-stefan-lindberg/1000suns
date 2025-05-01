using System.Collections.Generic;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public event System.Action OnConversationEnd;
    [System.Serializable]
    private struct ConversationEntry
    {
        public DialogueContent dialogueContent;
        public DialogueActor actor;
    }
    [SerializeField] private List<ConversationEntry> conversationList;
    private int currentDialogueIndex = 0;

    public enum DialogueActor {
        Player,
        CaveAvatar,
    }

    void OnEnable()
    {
        if (DialogueController.obj != null)
        {
            DialogueController.obj.OnDialogueClosed += OnDialogueCompleted;
            DialogueController.obj.OnDialogueClosing += OnDialogueClosing;
        }
    }

    private void OnDestroy()
    {
        if (DialogueController.obj != null)
        {
            DialogueController.obj.OnDialogueClosed -= OnDialogueCompleted;
            DialogueController.obj.OnDialogueClosing -= OnDialogueClosing;
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

    private void ShowNextDialogue()
    {
        if (currentDialogueIndex < conversationList.Count)
        {
            ConversationEntry entry = conversationList[currentDialogueIndex];
            bool leftMode = entry.actor == DialogueActor.Player;
            DialogueController.obj.ShowDialogue(entry.dialogueContent, leftMode);
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
