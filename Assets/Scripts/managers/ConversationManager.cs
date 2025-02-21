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

    private void Start()
    {
        if (DialogueController.obj != null)
        {
            DialogueController.obj.OnDialogueEnd += OnDialogueCompleted;
        }
    }

    private void OnDestroy()
    {
        if (DialogueController.obj != null)
        {
            DialogueController.obj.OnDialogueEnd -= OnDialogueCompleted;
        }
    }

    public void StartConversation()
    {
        if (conversationList.Count > 0)
        {
            currentDialogueIndex = 0;
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

    public void OnDialogueCompleted()
    {
        currentDialogueIndex++;

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
