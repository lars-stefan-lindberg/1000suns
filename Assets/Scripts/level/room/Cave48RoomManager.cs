using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave48RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameObject _hiddenFloor;
    [SerializeField] private GameEventId _hiddenFloorRevealed;
    [SerializeField] private GameEventId _sootFliedOff;
    [SerializeField] private GameEventId _cave52ConversationCompleted;
    [SerializeField] private GameEventId _guidingMushroomsGrown;
    [SerializeField] private GameEventId _cave50PostDreamRoom;
    [SerializeField] private GameObject _caveAvatarFlyOffTarget;
    [SerializeField] private GameObject _cutsceneTrigger;
    [SerializeField] private GameObject[] _guidingMushrooms;
    [SerializeField] private EventReference _mushroomGrowSfx;
    [SerializeField] private GameObject _growMushroomsCamera;

    private Coroutine _cutsceneCoroutine;

    void Start()
    {
        if(GameManager.obj.HasEvent(_guidingMushroomsGrown)) {
            foreach (var mushroom in _guidingMushrooms)
            {
                mushroom.GetComponent<SpriteRenderer>().enabled = true;
                mushroom.GetComponent<LightSprite2DFadeManager>().SetFadedInState();
            }
        }

        //If not following player -> assuming that we are Eli
        if(!CaveAvatar.obj.IsFollowingPlayer) {
            if(GameManager.obj.HasEvent(_sootFliedOff)) {
                if(GameManager.obj.HasEvent(_cave52ConversationCompleted)) {
                    CaveAvatar.obj.SetStartingPositionInRoom52AfterConversation();
                } else {
                    CaveAvatar.obj.SetStartingPositionInRoom52BeforeConversation();
                }
            }
            else
                CaveAvatar.obj.SetStartingPositionInRoom48();
        }
        if(GameManager.obj.HasEvent(_hiddenFloorRevealed)) {
            _hiddenFloor.SetActive(false);
        }
        if(GameManager.obj.HasEvent(_sootFliedOff))
            _cutsceneTrigger.SetActive(false);
    }

    public void OnHiddenFloorRevealed() {
        GameManager.obj.RegisterEvent(_hiddenFloorRevealed);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void StartCutscene() {
        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene() {
        PlayerBlobMovement.obj.Freeze();
        
        yield return new WaitForSeconds(2f);

        CaveAvatar.obj.SetTarget(_caveAvatarFlyOffTarget.transform);

        yield return new WaitForSeconds(3f);

        PlayerBlobMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_sootFliedOff);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        yield return null;
    }

    public void GrowGuidingMushrooms() {
        if(!GameManager.obj.HasEvent(_cave50PostDreamRoom))
            return;
        if(GameManager.obj.HasEvent(_guidingMushroomsGrown))
            return;
        
        _cutsceneCoroutine = StartCoroutine(GrowMushrooms());
    }

    private IEnumerator GrowMushrooms() {
        PauseMenuManager.obj.RegisterSkippable(this);
        var playerType = PlayerManager.obj.GetActivePlayerType();
        PlayerManager.obj.FreezePlayer(playerType);

        yield return new WaitForSeconds(0.5f);

        _growMushroomsCamera.SetActive(true);

        yield return new WaitForSeconds(2f);

        foreach (var mushroom in _guidingMushrooms)
        {
            mushroom.GetComponent<SpriteRenderer>().enabled = true;
            mushroom.GetComponent<SpriteRenderer>().sprite = null;
            mushroom.GetComponent<Animator>().enabled = true;
            SoundFXManager.obj.PlayAtPosition(_mushroomGrowSfx, mushroom.transform.position);
            
            LightSprite2DFadeManager lightFadeManager = mushroom.GetComponent<LightSprite2DFadeManager>();
            lightFadeManager.StartFadeIn();
            
            yield return new WaitForSeconds(1f);
        }

        _growMushroomsCamera.SetActive(false);
        yield return new WaitForSeconds(2f);

        PauseMenuManager.obj.UnregisterSkippable();
        PlayerManager.obj.UnfreezePlayer(playerType);
        GameManager.obj.RegisterEvent(_guidingMushroomsGrown);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }

        foreach (var mushroom in _guidingMushrooms)
        {
            mushroom.GetComponent<SpriteRenderer>().enabled = true;
            mushroom.GetComponent<LightSprite2DFadeManager>().SetFadedInState();
        }

        _growMushroomsCamera.SetActive(false);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        var playerType = PlayerManager.obj.GetActivePlayerType();
        PlayerManager.obj.UnfreezePlayer(playerType);
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }
}
