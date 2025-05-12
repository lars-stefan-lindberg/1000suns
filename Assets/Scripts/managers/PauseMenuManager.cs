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
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _firstSelectedPauseMenuItem;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private GameObject _persistentGameplay;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _ambienceSlider;
    [SerializeField] private TextMeshProUGUI _collectibleCountText;
    [SerializeField] private GameObject _pauseMainMenu;
    [SerializeField] private GameObject _keyboardConfigMenu;
    [SerializeField] private AutoScrollRect _keyboardConfigAutoScroll;
    [SerializeField] private TextMeshProUGUI _keyboardConfigInstructionsConfirmActionKeyText;
    [SerializeField] private Button _quitButton;
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
    public InputActionAsset actions;
    public InputActionReference confirmActionReference;
    public InputActionReference resetButtonActionReference;
    
    private string confirmActionKeyboardDisplayString;

    private bool _isPaused = false;
    // Track if the music volume slider is currently selected
    private bool _isMusicSliderSelected = false;

    void Awake() {
        obj = this;

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
        if(!GameEventManager.obj.IsPauseAllowed)
            return;
        if (context.performed)
        {
            if(_isPaused) {
                ResumeGame();
            } else {
                SoundFXManager.obj.PlayUIBack();
                PlayerManager.obj.DisablePlayerMovement();
                PlayerStatsManager.obj.PauseTimer();
                
                // Store the current music volume as the player's preferred volume
                SoundMixerManager.obj.SetPlayerPreferredMusicVolume(SoundMixerManager.obj.GetMusicVolume());
                
                // Apply the muffled effect - this will immediately set the volume to the muffled level
                StartCoroutine(SoundMixerManager.obj.StartMusicMuffle(0.5f));
                
                // Set the pause state
                _isPaused = true;
                
                // Set the time scale to 0 to pause the game
                Time.timeScale = 0f;
                
                // Enable the pause menu UI
                _pauseMenu.SetActive(true);
                
                // Set the music slider to reflect the player's preferred volume (not the muffled volume)
                _musicSlider.value = SoundMixerManager.obj.GetPlayerPreferredMusicVolume();
                
                _soundFXSlider.value = SoundMixerManager.obj.GetSoundFXVolume();
                _ambienceSlider.value = SoundMixerManager.obj.GetAmbienceVolume();
                
                _collectibleCountText.text = CollectibleManager.obj.GetNumberOfCollectiblesPicked().ToString();
                
                EventSystem.current.SetSelectedGameObject(_firstSelectedPauseMenuItem);
            }
        }
    }

    public void ResumeGame() {
        // Only resume if we're actually paused
        if (_isPaused) {
            SoundFXManager.obj.PlayUIConfirm();
            // Apply the unmuffled effect - this will immediately restore the volume to the player's preferred level
            StartCoroutine(SoundMixerManager.obj.StartMusicUnmuffle(0.5f));
            
            // Set the pause state
            _isPaused = false;
            
            // Set the time scale back to 1 to resume the game
            Time.timeScale = 1f;
            
            // Disable the pause menu UI
            _pauseMenu.SetActive(false);
            
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
        

    public void QuitButtonHandler() {
        _quitButton.interactable = false;
        SoundFXManager.obj.PlayUIBack();
        LevelTracker.obj.TrackQuitFromPauseMenu(SceneManager.GetActiveScene().name); 
        Quit();
    }

    public void Quit() {
        Time.timeScale = 1f;
        StartCoroutine(QuitCoroutine());
    }

    private IEnumerator QuitCoroutine() {
        float masterVolume = SoundMixerManager.obj.GetMasterVolume();

        SceneFadeManager.obj.StartFadeOut();
        StartCoroutine(SoundMixerManager.obj.StartMasterFade(1f, 0));
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        while(SoundMixerManager.obj.GetMasterVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();
        AmbienceManager.obj.StopAmbience();
        SceneManager.LoadScene(_titleScreen.SceneName);
        Scene titleScreen = SceneManager.GetSceneByName(_titleScreen.SceneName);
        while(!titleScreen.isLoaded) {
            yield return null;
        }
        while(LevelManager.obj.isRunningAfterSceneLoaded) {
            yield return null;
        }
        SoundMixerManager.obj.SetMasterVolume(masterVolume);
        Destroy(_persistentGameplay);
    }

    public void ChangeMusicVolume(float volume) {
        SoundFXManager.obj.PlayUISlider();
        
        // If this is the first time the music slider is used after pausing,
        // temporarily restore the music clarity for better feedback
        if (!_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = true;
            StartCoroutine(SoundMixerManager.obj.TemporarilyRestoreMusicForVolumeAdjustment(0.3f));
        }
        
        // Set the player's preferred music volume directly
        SoundMixerManager.obj.SetPlayerPreferredMusicVolume(volume);
    }

    public void ChangeSoundFxVolume(float volume) {
        SoundFXManager.obj.PlayUISlider();
        
        // If we were previously adjusting music, re-muffle it when switching to another slider
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
        }
        
        SoundMixerManager.obj.SetSoundFXVolume(volume);
    }
    
    public void ChangeAmbienceVolume(float volume) {
        SoundFXManager.obj.PlayUISlider();
        
        // If we were previously adjusting music, re-muffle it when switching to another slider
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
        }
        
        SoundMixerManager.obj.SetAmbienceVolume(volume);
    }

    public void ShowKeyboardConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();
        
        // If we were previously adjusting music, re-muffle it when switching to another menu
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
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

        SoundFXManager.obj.PlayUIBack();
        EventSystem.current.SetSelectedGameObject(null);
        _keyboardConfigAutoScroll.ResetScrollRect();

        _keyboardConfigMenu.SetActive(false);
        _pauseMainMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_keyboardConfigMenuButton.gameObject);
    }

    public void ShowControllerConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();
        
        // If we were previously adjusting music, re-muffle it when switching to another menu
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
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

        SoundFXManager.obj.PlayUIBack();
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
            if(_isPaused)
                ResumeGame();
        } else if(_keyboardConfigMenu.activeSelf) {
            LeaveKeyboardConfigMenu();
        } else if(_controllerConfigMenu.activeSelf) {
            LeaveControllerConfigMenu();
        }
    }

    public void RetryRoomHandler() {
        ResumeGame();
        Reaper.obj.KillPlayerGeneric();
    }

    // Called when a UI element is selected
    public void OnUIElementSelected(GameObject selectedGameObject) {
        // Only process if we're paused and the selection has changed
        if (_isPaused && selectedGameObject != _lastSelectedGameObject) {
            _lastSelectedGameObject = selectedGameObject;
            
            // Check if the music slider is being selected
            if (selectedGameObject == _musicSlider.gameObject) {
                _isMusicSliderSelected = true;
                StartCoroutine(SoundMixerManager.obj.TemporarilyRestoreMusicForVolumeAdjustment(0.3f));
            }
            // Check if we're moving away from the music slider
            else if (_isMusicSliderSelected && selectedGameObject != _musicSlider.gameObject) {
                _isMusicSliderSelected = false;
                StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
            }
        }
    }

    // Called when the music slider is deselected
    public void OnMusicSliderDeselected() {
        if (_isMusicSliderSelected && _isPaused) {
            _isMusicSliderSelected = false;
            
            // Re-apply the muffled effect when the slider is no longer being used
            StartCoroutine(SoundMixerManager.obj.ReapplyMusicMuffleAfterVolumeAdjustment(0.3f));
        }
    }
}
