using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave25RoomManager : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameEventId _cutsceneCompleted;
    [SerializeField] private Transform _sootFlyTarget1;
    [SerializeField] private Transform _sootFlyTarget2;
    [SerializeField] private FloatyPlatform _floatingPlatform;
    [SerializeField] private GameObject _cutsceneCamera;

    public void StartCutscene() {
        if(GameManager.obj.HasEvent(_cutsceneCompleted)) {
            return;
        }
        PlayerMovement.obj.Freeze();

        StartCoroutine(StartCutsceneCoroutine());
    }

    private IEnumerator StartCutsceneCoroutine() {
        _cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2f);
        _conversationManager.enabled = true;
        _conversationManager.OnConversationEnd += OnConversationCompleted;

        CaveAvatar.obj.SetTarget(_sootFlyTarget1);
        yield return new WaitForSeconds(2f);
        CaveAvatar.obj.SetTarget(_sootFlyTarget2);
        yield return new WaitForSeconds(0.3f);
        _floatingPlatform.PlayImpactSfx();
        _floatingPlatform.MovePlatform(true, 1.5f);
        _floatingPlatform.StartFallCountDown();
        yield return new WaitForSeconds(1f);

        _conversationManager.StartConversation();
    }
    
    
    private void OnConversationCompleted() { 
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.enabled = false;

        StartCoroutine(OnConversationCompletedCoroutine());
    }
    
    private IEnumerator OnConversationCompletedCoroutine() { 
        _cutsceneCamera.SetActive(false);
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.IsFollowingPlayer = true;
        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        yield return null;
    }


    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
