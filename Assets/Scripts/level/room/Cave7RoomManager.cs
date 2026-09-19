using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Cave7RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _tutorialCompleted;
    [SerializeField] private GameEventId _seenPauseMenuPowers;
    [SerializeField] private TutorialStrip _pauseMenuPowersTutorial;
    [SerializeField] private TutorialStrip _tutorialStrip;
    [SerializeField] private LocalizedString _basicMovementString;
    [SerializeField] private List<InputActionReference> _basicMovementActions;
    [SerializeField] private List<InputIconManager.Direction> _basicMovementDirections;
    
    [SerializeField] private LocalizedString _fullyChargedString;
    [SerializeField] private List<InputActionReference> _fullyChargedActions;
    [SerializeField] private List<InputIconManager.Direction> _fullyChargedDirections;

    void Start() {
        var steps = new List<ConditionalTutorialStep>
        {
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _basicMovementString,
                    inputActions = _basicMovementActions,
                    inputDirections = _basicMovementDirections
                },
                condition = null // No condition = default step
            },
            
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _fullyChargedString,
                    inputActions = _fullyChargedActions,
                    inputDirections = _fullyChargedDirections
                },
                condition = () => PlayerPush.obj != null && PlayerPush.obj.IsFullyCharged()
            }
        };
        
        _tutorialStrip.InitializeConditional(steps);
    }

    public void ShowTutorialStrip() {
        if(GameManager.obj.HasEvent(_tutorialCompleted))
            return;
        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        if(GameManager.obj.HasEvent(_tutorialCompleted))
            return;
        _tutorialStrip.Hide();
        GameManager.obj.RegisterEvent(_tutorialCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void ShowPauseMenuPowersTutorialStrip() {
        if(GameManager.obj.HasEvent(_seenPauseMenuPowers))
            return;
        _pauseMenuPowersTutorial.Show();
    }

    public void HidePauseMenuPowersTutorialStrip() {
        if(GameManager.obj.HasEvent(_seenPauseMenuPowers))
            return;
        _pauseMenuPowersTutorial.Hide();
        GameManager.obj.RegisterEvent(_seenPauseMenuPowers);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
