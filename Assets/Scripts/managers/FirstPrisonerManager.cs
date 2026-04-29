using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPrisonerManager : MonoBehaviour
{
    [SerializeField] private GameEventId _firstPrisonerFightEnded;
    [SerializeField] private GameObject _bossGameObjects;

    void Start() {
        if(GameManager.obj.HasEvent(_firstPrisonerFightEnded)) {
            _bossGameObjects.SetActive(false);
        }
    }

    public void EndFight() {
        MusicManager.obj.EndCurrentTrack();
        GameManager.obj.RegisterEvent(_firstPrisonerFightEnded);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
