using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager obj;
    public bool isNavigatingToMenu = true;
    private ISkippable _skippable;

    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _pauseMenuBg;
    [SerializeField] private PauseMainScreen _pauseMainScreen;
    [SerializeField] private GameObject _skipCutsceneMenuItem;
    [SerializeField] private GameObject _retryRoomMenuItem;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _skipCutsceneButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private OptionsScreen _optionsScreen;
    [SerializeField] private GameObject _optionsMenuAudioButton;
    [SerializeField] private AudioScreen _audioScreen;
    [SerializeField] private GameObject _optionsMenuControllerButton;
    [SerializeField] private ControllerScreen _controllerConfigScreen;
    [SerializeField] private GameObject _optionsMenuKeyboardButton;
    [SerializeField] private KeyboardScreen _keyboardConfigScreen;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private GameObject _persistentGameplay;
    [SerializeField] private InputActionReference _cancelActionReference;
    
    private bool _isPaused = false;
    private int _escapeKeyBindingIndex = -1;
    private Sequence _menuTransitionSequence;
    private bool _isTransitioning = false;
    private Stack<UIScreen> screenStack = new();

    void Awake() {
        obj = this;

        SetCanvasCamera(_pauseMenuBg.GetComponent<Canvas>());
        SetCanvasCamera(_pauseMainScreen.GetComponent<Canvas>());
        SetCanvasCamera(_optionsScreen.GetComponent<Canvas>());
        SetCanvasCamera(_audioScreen.GetComponent<Canvas>());
        SetCanvasCamera(_controllerConfigScreen.GetComponent<Canvas>());
        SetCanvasCamera(_keyboardConfigScreen.GetComponent<Canvas>());
    }

    void Start()
    {
       
    }
    

    private void SetCanvasCamera(Canvas canvas) {
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
    }

    void OnDestroy() {
        _cancelActionReference.action.performed -= OnCancel;
        EnableEscapeInUIControls();
        obj = null;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(!GameManager.obj.IsPauseAllowed)
            return;
        if (context.performed)
        {
            if(_isPaused) {
                UISoundPlayer.obj.PlaySelect();
                ResumeGame();
            } else {
                _cancelActionReference.action.performed += OnCancel;
                DisableEscapeInUIControls();
                UISoundPlayer.obj.PlayBack();
                PlayerManager.obj.DisablePlayerMovement();
                PlayerStatsManager.obj.PauseTimer();
                
                AudioStateManager.obj.SetPaused(true);
                
                // Set the pause state
                _isPaused = true;
                
                // Set the time scale to 0 to pause the game
                Time.timeScale = 0f;
                
                // Enable the pause menu UI
                _pauseMenu.SetActive(true);
                OpenScreen(_pauseMainScreen);
                
                if(_skippable != null) {
                    _skipCutsceneMenuItem.SetActive(true);
                    _retryRoomMenuItem.SetActive(false);
                    Navigation resumeButtonNav = _resumeButton.navigation;
                    resumeButtonNav.selectOnDown = _skipCutsceneButton;
                    _resumeButton.navigation = resumeButtonNav;
                    Navigation optionsButtonNavigation = _optionsButton.navigation;
                    optionsButtonNavigation.selectOnUp = _skipCutsceneButton;
                    _optionsButton.navigation = optionsButtonNavigation;
                } else {
                    _skipCutsceneMenuItem.SetActive(false);
                    _retryRoomMenuItem.SetActive(true);
                    Navigation resumeButtonNav = _resumeButton.navigation;
                    resumeButtonNav.selectOnDown = _retryButton;
                    _resumeButton.navigation = resumeButtonNav;
                    Navigation optionsButtonNavigation = _optionsButton.navigation;
                    optionsButtonNavigation.selectOnUp = _retryButton;
                    _optionsButton.navigation = optionsButtonNavigation;
                }
            }
        }
    }

    public void OpenScreen(UIScreen newScreen, GameObject triggerButton = null)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;

        UIScreen current = screenStack.Count > 0 ? screenStack.Peek() : null;

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();
        _menuTransitionSequence.SetUpdate(true);

        if(triggerButton != null)
            current.SetBackSelectable(triggerButton);
        // Hide current
        if (current != null)
            _menuTransitionSequence.Append(current.Hide());

        // Show next
        _menuTransitionSequence.Append(newScreen.Show());

        _menuTransitionSequence.OnComplete(() =>
        {
            screenStack.Push(newScreen);
            _isTransitioning = false;
        });
    }

    public void GoBack()
    {
        if (_isTransitioning || screenStack.Count == 0)
            return;

        _isTransitioning = true;

        UIScreen current = screenStack.Pop();
        UIScreen previous = screenStack.Peek();

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();
        _menuTransitionSequence.SetUpdate(true);

        _menuTransitionSequence.Append(current.Hide());

        _menuTransitionSequence.Append(previous.Show());

        _menuTransitionSequence.OnComplete(() =>
        {
            _isTransitioning = false;
        });
    }

    public void OnResumeButtonClick() {
        UISoundPlayer.obj.PlaySelect(); 
        ResumeGame();
    }

    public void OnRetryButtonClick() {
        UISoundPlayer.obj.PlaySelect(); 
        ResumeGame();
        Reaper.obj.KillAllPlayersGeneric();
    }

    public void OnSkipCutsceneButtonClick() {
        UISoundPlayer.obj.PlaySelect();
        Time.timeScale = 1f;
        StartCoroutine(SkipCutsceneCoroutine());
    }

    public void OnOptionsButtonClick() {
        UISoundPlayer.obj.PlaySelect();
        OpenScreen(_optionsScreen, _optionsButton.gameObject);
    }

    public void OnQuitButtonClick() {
        _quitButton.interactable = false;
        UISoundPlayer.obj.PlaySelect();
        LevelTracker.obj.TrackQuitFromPauseMenu(SceneManager.GetActiveScene().name); 
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        SaveManager.obj.SetActiveSaveProfile(0);
        Quit();
    }

    public void OnAudioMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        AudioStateManager.obj.RestoreMusic();
        OpenScreen(_audioScreen, _optionsMenuAudioButton);
    }

    public void GoBackFromAudioMenu() {
        AudioStateManager.obj.SetPaused(true);
        GoBack();
    }

    public void OnControllerMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();

        OpenScreen(_controllerConfigScreen, _optionsMenuControllerButton);
    }

    public void OnKeyboardMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();

        OpenScreen(_keyboardConfigScreen, _optionsMenuKeyboardButton);
    }

    public void RegisterSkippable(ISkippable skippable)
    {
        _skippable = skippable;
    }

    public void UnregisterSkippable() {
        _skippable = null;
    }

    public void ResumeGame() {
        // Only resume if we're actually paused
        if (_isPaused) {
             _cancelActionReference.action.performed -= OnCancel;
            EnableEscapeInUIControls();
            EventSystem.current.SetSelectedGameObject(null);

            UIScreen uiScreen = screenStack.Pop();
            uiScreen.Hide();
            screenStack.Clear();
            _pauseMainScreen.SetBackSelectable(null);
            _pauseMenu.SetActive(false);

            AudioStateManager.obj.SetPaused(false);
            
            DialogueController dialogueController = FindActiveDialogueController();
            if(dialogueController != null && dialogueController.IsDisplayed()) {
                dialogueController.FocusDialogue();
            } else if(!PlayerManager.obj.IsFrozen()) {
                PlayerManager.obj.EnablePlayerMovement();
            }

            // Set the pause state
            _isPaused = false;
            // Set the time scale back to 1 to resume the game
            Time.timeScale = 1f;
            PlayerStatsManager.obj.ResumeTimer();
        }
    }

    private IEnumerator SkipCutsceneCoroutine() {
        GameManager.obj.IsPauseAllowed = false;
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }   
        yield return new WaitForSeconds(0.3f);
        _skippable.RequestSkip();
        UnregisterSkippable();
        ResumeGame();
        yield return null;
    }

    private DialogueController FindActiveDialogueController() {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] gameObjects = activeScene.GetRootGameObjects();
        GameObject dialogueObject = null;
        try {
            dialogueObject = gameObjects.First(gameObject => gameObject.CompareTag("Dialogue"));
        }catch(Exception e) {}

        if(dialogueObject == null) {
            return null;
        }
        return dialogueObject.GetComponentInChildren<DialogueController>();
    }

    public void Quit() {
        Time.timeScale = 1f;
        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine() {
        MusicManager.obj.Stop();
        AmbienceManager.obj.Stop();

        UIScreen uiScreen = screenStack.Pop();
        uiScreen.Hide();
        screenStack.Clear();
        _pauseMainScreen.SetBackSelectable(null);
        
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());

        //Give music some time to fade out before "unmuffling" the music
        yield return new WaitForSeconds(1.5f);
        AudioStateManager.obj.RestoreMusic();
        _pauseMenu.SetActive(false);
        
        SceneManager.LoadScene(_titleScreen.SceneName);
        Scene titleScreen = SceneManager.GetSceneByName(_titleScreen.SceneName);
        while(!titleScreen.isLoaded) {
            yield return null;
        }

        //Now it should be safe to kill ongoing sfx, and turn sfx on again
        AudioStateManager.obj.StopSfxEvents();
        AudioStateManager.obj.RestoreSfx();

        while(LevelManager.obj.isRunningAfterSceneLoaded) {
            yield return null;
        }



        Destroy(_persistentGameplay);
    }

    private void OnCancel(InputAction.CallbackContext context) {
        if(screenStack.Count > 1) {
            UISoundPlayer.obj.PlayBack();
            UIScreen screen = screenStack.Peek();
            if(screen is AudioScreen)
                GoBackFromAudioMenu();
            else
                GoBack();
        } else if(screenStack.Count == 1) {
            UISoundPlayer.obj.PlaySelect();
            ResumeGame();
        }
    }

    private void DisableEscapeInUIControls() {
        if (_cancelActionReference == null || _cancelActionReference.action == null)
            return;

        var cancelAction = _cancelActionReference.action;
        var bindings = cancelAction.bindings;

        for (int i = 0; i < bindings.Count; i++) {
            if (bindings[i].path == "<Keyboard>/escape") {
                _escapeKeyBindingIndex = i;
                cancelAction.ApplyBindingOverride(i, "");
                break;
            }
        }
    }

    private void EnableEscapeInUIControls() {
        if (_cancelActionReference == null || _cancelActionReference.action == null || _escapeKeyBindingIndex == -1)
            return;

        var cancelAction = _cancelActionReference.action;
        cancelAction.RemoveBindingOverride(_escapeKeyBindingIndex);
        _escapeKeyBindingIndex = -1;
    }
}
