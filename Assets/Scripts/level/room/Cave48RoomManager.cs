using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave48RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _hiddenFloor;
    [SerializeField] private GameEventId _hiddenFloorRevealed;
    [SerializeField] private GameEventId _sootFliedOff;
    [SerializeField] private GameObject _caveAvatarFlyOffTarget;
    [SerializeField] private GameObject _caveAvatarStartingPos;
    [SerializeField] private GameObject _cutsceneTrigger;

    void Start()
    {
        //If not following player -> assuming that we are Eli
        if(!CaveAvatar.obj.IsFollowingPlayer) {
            if(GameManager.obj.HasEvent(_sootFliedOff))
                CaveAvatar.obj.SetStartingPositionInRoom52();
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
}
