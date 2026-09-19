using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave28RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameEventId _haveSeenCameraLookTutorial;
    [SerializeField] private GameObject _secretWall;
    [SerializeField] private TutorialStrip _tutorialStrip;

    void Start()
    {
        if(GameManager.obj.HasEvent(_secretRevealed)) {
            _secretWall.SetActive(false);
        }
        if(GameManager.obj.HasEvent(_haveSeenCameraLookTutorial))
            _tutorialStrip.gameObject.SetActive(false);
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
    }

    public void ShowTutorialStrip() {
        if(GameManager.obj.HasEvent(_haveSeenCameraLookTutorial))
            return;
        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        if(GameManager.obj.HasEvent(_haveSeenCameraLookTutorial))
            return;
        _tutorialStrip.Hide();
        GameManager.obj.RegisterEvent(_haveSeenCameraLookTutorial);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
    
    public void HideTutorialStripQuick() {
        if(GameManager.obj.HasEvent(_haveSeenCameraLookTutorial))
            return;
        _tutorialStrip.HideQuick();
    }
}
