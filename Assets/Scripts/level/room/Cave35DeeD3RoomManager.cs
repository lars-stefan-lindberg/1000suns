using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave35DeeD3RoomManager : MonoBehaviour
{
    [SerializeField] private TutorialStrip _tutorialStrip;

    public void ShowTutorialStrip() {
        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        _tutorialStrip.Hide();
    }
    
    public void HideTutorialStripQuick() {
        _tutorialStrip.HideQuick();
    }
}
