using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager obj;
    public bool isNavigatingToMenu = true;
    private ISkippable _skippable;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _firstSelectedPauseMenuItem;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private GameObject _persistentGameplay;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private TextMeshProUGUI _collectibleCountText;
    [SerializeField] private GameObject _pauseMainMenu;
    [SerializeField] private GameObject _keyboardConfigMenu;
    [SerializeField] private AutoScrollRect _keyboardConfigAutoScroll;
    [SerializeField] private TextMeshProUGUI _keyboardConfigInstructionsConfirmActionKeyText;
    [SerializeField] private Button _quitButton;
    [SerializeField] private GameObject _skipCutsceneOption;
    [SerializeField] private GameObject _retryRoomOption;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _firstKeyboardMenuButton;
    [SerializeField] private Button _keyboardConfigMenuBackButton;
    [SerializeField] private Button _keyboardConfigMenuButton;
    [SerializeField] private GameObject _controllerConfigMenu;
    [SerializeField] private GameObject _controllerConfigMenuShowConfig;
    [SerializeField] private GameObject _controllerConfigMenuShowAttachController;
    [SerializeField] private Button _controllerConfigMenuButton;
    [SerializeField] private Button _controllerConfigMenuBackButton;
    [SerializeField] private Button _firstControllerMenuButton;
    [SerializeField] private Image _gamepadConfigInstructionsConfirmActionKeyIcon;
    [SerializeField] private Image _gamepadConfigInstructionsResetButtonActionKeyIcon;
    [SerializeField] private TextMeshProUGUI _roomNumber;
    public InputActionAsset actions;
    public InputActionReference confirmActionReference;
    public InputActionReference resetButtonActionReference;
    
    private string confirmActionKeyboardDisplayString;

    private bool _isPaused = false;
    // Track if the music volume slider is currently selected
    private bool _isMusicSliderSelected = false;
    private bool _muteSliderSfx = false;

    void Awake() {
        obj = this;

        Canvas canvas = _pauseMenu.GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
    }

    // Track the last selected game object to detect changes
    private GameObject _lastSelectedGameObject = null;

    void Update() {
        // Check if we're paused and if the selected UI element has changed
        if (_isPaused) {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            
            // If selection changed
            if (currentSelected != _lastSelectedGameObject) {
                OnUIElementSelected(currentSelected);
                _lastSelectedGameObject = currentSelected;
            }
        }
    }

    void OnDestroy() {
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
                _roomNumber.text = SceneManager.GetActiveScene().name;
                
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
                _pauseMainMenu.SetActive(true);
                
                // Set the music slider to reflect the player's preferred volume (not the muffled volume)
                _muteSliderSfx = true;
                _musicSlider.value = AudioOptions.obj.MusicStep;
                
                _soundFXSlider.value = AudioOptions.obj.SfxStep;
                _muteSliderSfx = false;
                
                _collectibleCountText.text = CollectibleManager.obj.GetNumberOfCollectiblesPicked().ToString();

                if(_skippable != null) {
                    _skipCutsceneOption.SetActive(true);
                    _retryRoomOption.SetActive(false);
                    Navigation resumeButtonNav = _resumeButton.navigation;
                    resumeButtonNav.selectOnDown = _skipCutsceneOption.GetComponent<Button>();
                    _resumeButton.navigation = resumeButtonNav;
                    Navigation musicSliderNavigation = _musicSlider.navigation;
                    musicSliderNavigation.selectOnUp = _skipCutsceneOption.GetComponent<Button>();
                    _musicSlider.navigation = musicSliderNavigation;
                } else {
                    _skipCutsceneOption.SetActive(false);
                    _retryRoomOption.SetActive(true);
                    Navigation resumeButtonNav = _resumeButton.navigation;
                    resumeButtonNav.selectOnDown = _retryRoomOption.GetComponent<Button>();
                    _resumeButton.navigation = resumeButtonNav;
                    Navigation musicSliderNavigation = _musicSlider.navigation;
                    musicSliderNavigation.selectOnUp = _retryRoomOption.GetComponent<Button>();
                    _musicSlider.navigation = musicSliderNavigation;
                }
                
                EventSystem.current.SetSelectedGameObject(_firstSelectedPauseMenuItem);
            }
        }
    }

    public void OnResumeButtonClick() {
        UISoundPlayer.obj.PlaySelect(); 
        ResumeGame();
    }

    public void ResumeGame() {
        // Only resume if we're actually paused
        if (_isPaused) {
            AudioStateManager.obj.SetPaused(false);
            
            // Set the pause state
            _isPaused = false;
            
            // Set the time scale back to 1 to resume the game
            Time.timeScale = 1f;
            
            // Disable the pause menu UIs
            _pauseMenu.SetActive(false);
            _pauseMainMenu.SetActive(false);
            _keyboardConfigMenu.SetActive(false);
            _controllerConfigMenu.SetActive(false);
            
            // Reset music slider selection state
            _isMusicSliderSelected = false;
            _lastSelectedGameObject = null;
            
            EventSystem.current.SetSelectedGameObject(null);

            DialogueController dialogueController = FindActiveDialogueController();
            if(dialogueController != null && dialogueController.IsDisplayed()) {
                dialogueController.FocusDialogue();
            } else if(TutorialDialogManager.obj != null && !TutorialDialogManager.obj.tutorialCompleted) {
                TutorialDialogManager.obj.Focus();
            } else if(!PlayerManager.obj.IsFrozen()) {
                PlayerManager.obj.EnablePlayerMovement();
            }
            
            isNavigatingToMenu = true;
            PlayerStatsManager.obj.ResumeTimer();
        }
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

    public void RegisterSkippable(ISkippable skippable)
    {
        _skippable = skippable;
    }

    public void UnregisterSkippable() {
        _skippable = null;
    }

    public void OnSkipCutsceneClick() {
        UISoundPlayer.obj.PlaySelect();
        Time.timeScale = 1f;
        StartCoroutine(SkipCutsceneCoroutine());
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
        

    public void QuitButtonHandler() {
        _quitButton.interactable = false;
        UISoundPlayer.obj.PlayBack();
        LevelTracker.obj.TrackQuitFromPauseMenu(SceneManager.GetActiveScene().name); 
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        Quit();
    }

    public void Quit() {
        Time.timeScale = 1f;
        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine() {
        //Disable custom ui input handler so it doesn't clash with main menu customUIinputhandler
        _pauseMenu.GetComponent<CustomUIInputHandler>().enabled = false;

        MusicManager.obj.Stop();
        AudioStateManager.obj.QuitGame();
        AmbienceManager.obj.Stop();
        
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        
        AudioStateManager.obj.SetPaused(false);

        SceneManager.LoadScene(_titleScreen.SceneName);
        Scene titleScreen = SceneManager.GetSceneByName(_titleScreen.SceneName);
        while(!titleScreen.isLoaded) {
            yield return null;
        }
        while(LevelManager.obj.isRunningAfterSceneLoaded) {
            yield return null;
        }

        Destroy(_persistentGameplay);
    }

    public void ChangeMusicVolume(float volume) {
        if(!_muteSliderSfx)
            UISoundPlayer.obj.PlaySliderTick();
        
        // If this is the first time the music slider is used after pausing,
        // temporarily restore the music clarity for better feedback
        if (!_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = true;
            AudioStateManager.obj.RestoreMusic();
        }
        
        // Set the player's preferred music volume directly
        // TODO: implement this with FMOD
        AudioOptions.obj.SetMusicStep(volume);
    }

    public void ChangeSoundFxVolume(float volume) {
        if(!_muteSliderSfx)
            UISoundPlayer.obj.PlaySliderTick();
        
        // If we were previously adjusting music, re-muffle it when switching to another slider
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            AudioStateManager.obj.SetPaused(true);
        }
        
        // TODO: implement this with FMOD
        AudioOptions.obj.SetSfxStep(volume);
    }

    public void ShowKeyboardConfigMenu() {
        UISoundPlayer.obj.PlaySelect();
        
        // If we were previously adjusting music, re-muffle it when switching to another menu
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            AudioStateManager.obj.SetPaused(true);
        }

        _pauseMainMenu.SetActive(false);

        _keyboardConfigMenu.SetActive(true);
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);

        //Get display string from confirm key
        _keyboardConfigInstructionsConfirmActionKeyText.text = confirmActionKeyboardDisplayString;
        EventSystem.current.SetSelectedGameObject(_firstKeyboardMenuButton.gameObject);
    }

    public void LeaveKeyboardConfigMenu() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        UISoundPlayer.obj.PlayBack();
        EventSystem.current.SetSelectedGameObject(null);
        _keyboardConfigAutoScroll.ResetScrollRect();

        _keyboardConfigMenu.SetActive(false);
        _pauseMainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_keyboardConfigMenuButton.gameObject);
    }

    public void ShowControllerConfigMenu() {
        UISoundPlayer.obj.PlaySelect();
        
        // If we were previously adjusting music, re-muffle it when switching to another menu
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            AudioStateManager.obj.SetPaused(true);
        }

        _pauseMainMenu.SetActive(false);

        var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                actions.LoadBindingOverridesFromJson(rebinds);

        Sprite confirmButtonSprite = GamepadIconManager.obj.GetIcon(confirmActionReference.action);
        _gamepadConfigInstructionsConfirmActionKeyIcon.sprite = confirmButtonSprite;

        Sprite resetButtonSprite = GamepadIconManager.obj.GetIcon(resetButtonActionReference.action);
        _gamepadConfigInstructionsResetButtonActionKeyIcon.sprite = resetButtonSprite;

        if(Gamepad.current != null) {
            _controllerConfigMenu.SetActive(true);
            _controllerConfigMenuShowConfig.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_firstControllerMenuButton.gameObject);
        } else {
            _controllerConfigMenu.SetActive(true);
            _controllerConfigMenuShowAttachController.SetActive(true);
        }
    }

    public void LeaveControllerConfigMenu() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        UISoundPlayer.obj.PlayBack();
        _controllerConfigMenu.SetActive(false);
        _controllerConfigMenuShowAttachController.SetActive(false);
        _controllerConfigMenuShowConfig.SetActive(false);
        _pauseMainMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null); //Make sure that the event system "catches up", and can select the config menu button on the next line
        EventSystem.current.SetSelectedGameObject(_controllerConfigMenuButton.gameObject);
    }

    public void OnNavigateBack() {
        isNavigatingToMenu = true;
        
        // If we were previously adjusting music, reset the flag
        if (_isMusicSliderSelected) {
            _isMusicSliderSelected = false;
        }

        if(_pauseMainMenu.activeSelf) {
            if(_isPaused) {
                UISoundPlayer.obj.PlaySelect();
                ResumeGame();
            }
        } else if(_keyboardConfigMenu.activeSelf) {
            LeaveKeyboardConfigMenu();
        } else if(_controllerConfigMenu.activeSelf) {
            LeaveControllerConfigMenu();
        }
    }

    public void RetryRoomHandler() {
        ResumeGame();
        Reaper.obj.KillAllPlayersGeneric();
    }

    // Called when a UI element is selected
    public void OnUIElementSelected(GameObject selectedGameObject) {
        // Only process if we're paused and the selection has changed
        if (_isPaused && selectedGameObject != _lastSelectedGameObject) {
            _lastSelectedGameObject = selectedGameObject;
            
            // Check if the music slider is being selected
            if (selectedGameObject == _musicSlider.gameObject) {
                _isMusicSliderSelected = true;
                AudioStateManager.obj.RestoreMusic();
            }
            // Check if we're moving away from the music slider
            else if (_isMusicSliderSelected && selectedGameObject != _musicSlider.gameObject) {
                _isMusicSliderSelected = false;
                AudioStateManager.obj.SetPaused(true);
            }
        }
    }

    // Called when the music slider is deselected
    public void OnMusicSliderDeselected() {
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            
            // Re-apply the muffled effect when the slider is no longer being used
            AudioStateManager.obj.SetPaused(true);
        }
    }
}
