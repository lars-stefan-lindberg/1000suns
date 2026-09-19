using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Forest1RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _tutorialSeen;
    [SerializeField] private TutorialStrip _tutorialStrip;

    public void ShowTutorialStrip() {
        if(GameManager.obj.HasEvent(_tutorialSeen))
            return;
        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        if(GameManager.obj.HasEvent(_tutorialSeen))
            return;
        _tutorialStrip.Hide();
        GameManager.obj.RegisterEvent(_tutorialSeen);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
