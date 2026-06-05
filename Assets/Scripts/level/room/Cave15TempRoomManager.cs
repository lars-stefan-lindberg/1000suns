using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave15TempRoomManager : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    private Coroutine _onConversationCompletedCoroutine;

    void Start() {
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }

    public void SetupDialogue() {
        PlayerMovement.obj.Freeze();
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        _onConversationCompletedCoroutine = StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() {
        PlayerMovement.obj.UnFreeze();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        yield return null;
    }
}
