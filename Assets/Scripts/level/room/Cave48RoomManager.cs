using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave48RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _hiddenFloor;
    [SerializeField] private GameEventId _hiddenFloorRevealed;
    [SerializeField] private GameEventId _sootFliedOff;
    [SerializeField] private GameEventId _cave52ConversationCompleted;
    [SerializeField] private GameEventId _guidingMushroomsGrown;
    [SerializeField] private GameObject _caveAvatarFlyOffTarget;
    [SerializeField] private GameObject _cutsceneTrigger;
    [SerializeField] private GameObject[] _guidingMushrooms;
    [SerializeField] private EventReference _mushroomGrowSfx;

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
        if(GameManager.obj.HasEvent(_guidingMushroomsGrown))
            return;
        
        StartCoroutine(GrowMushrooms());
    }

    private IEnumerator GrowMushrooms() {
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
        GameManager.obj.RegisterEvent(_guidingMushroomsGrown);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        yield return null;
    }
}
