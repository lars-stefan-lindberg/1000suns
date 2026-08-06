using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FirstPrisonerManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _firstPrisonerFightEndedEli;
    [SerializeField] private GameEventId _firstPrisonerFightEndedDee;
    [SerializeField] private GameObject _bossGameObjects;
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private Tilemap _blockingWallTilemap;
    [SerializeField] private ConversationManager _conversationManager;

    private Coroutine _cutsceneCoroutine;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        
        if(caveTimeline == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_firstPrisonerFightEndedEli)) {
            _bossGameObjects.SetActive(false);
        } else if(caveTimeline == CaveTimelineId.Id.Dee && GameManager.obj.HasEvent(_firstPrisonerFightEndedDee)) {
            _bossGameObjects.SetActive(false);
        }
    }

    public void EndFight() {
        Color startColor = _blockingWallTilemap.color;
        startColor.a = 1f;
        _blockingWallTilemap.color = startColor;
        Color targetColor = startColor;
        targetColor.a = 0f;
        DOTween.To(() => _blockingWallTilemap.color, x => _blockingWallTilemap.color = x, targetColor, 1f).OnComplete(() => {
            _blockingWall.SetActive(false);
        });
        
        MusicManager.obj.EndCurrentTrack();
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            GameManager.obj.RegisterEvent(_firstPrisonerFightEndedEli);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            PauseMenuManager.obj.RegisterSkippable(this);
            ShadowTwinMovement.obj.Freeze();
            _conversationManager.OnConversationEnd += OnConversationCompleted;
            StartCoroutine(PlayDialogue());
        }
    }
    private IEnumerator PlayDialogue() {
        yield return new WaitForSeconds(2.5f);
        _conversationManager.StartConversation();
        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }

        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.HardStopConversation();
        _conversationManager.CleanUp();

        GameManager.obj.RegisterEvent(_firstPrisonerFightEndedDee);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        StartCoroutine(ResumeGameplay());
    }
    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private void OnConversationCompleted() {
        PauseMenuManager.obj.UnregisterSkippable();
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_firstPrisonerFightEndedDee);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
