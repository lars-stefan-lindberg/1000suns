using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    void Awake() {
        obj = this;

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
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
                PlayerMovement.obj.DisablePlayerMovement();
                PlayerStatsManager.obj.PauseTimer();

                _musicSlider.value = SoundMixerManager.obj.GetMusicVolume();
                _soundFXSlider.value = SoundMixerManager.obj.GetSoundFXVolume();
                _ambienceSlider.value = SoundMixerManager.obj.GetAmbienceVolume();

                _collectibleCountText.text = CollectibleManager.obj.GetNumberOfCollectiblesPicked().ToString();

                _isPaused = true;
                _pauseMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(_firstSelectedPauseMenuItem);
                Time.timeScale = 0f;
            }
        }
    }

    public void ResumeGame() {
        SoundFXManager.obj.PlayUIConfirm();

        EventSystem.current.SetSelectedGameObject(null);
        _pauseMenu.SetActive(false);
            
        Time.timeScale = 1f;
        if(DialogueController.obj != null && DialogueController.obj.IsDisplayed()) {
            DialogueController.obj.FocusDialogue();
        } else if(TutorialDialogManager.obj != null && !TutorialDialogManager.obj.tutorialCompleted) {
            TutorialDialogManager.obj.Focus();
        } else if(!PlayerMovement.obj.IsFrozen()) {
            PlayerMovement.obj.EnablePlayerMovement();
        }
        _isPaused = false;
        isNavigatingToMenu = true;

        PlayerStatsManager.obj.ResumeTimer();
    }

    public void QuitButtonHandler() {
        _quitButton.interactable = false;
        SoundFXManager.obj.PlayUIBack();
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
        SoundMixerManager.obj.SetMusicVolume(volume);
    }

    public void ChangeSoundFxVolume(float volume) {
        SoundFXManager.obj.PlayUISlider();
        SoundMixerManager.obj.SetSoundFXVolume(volume);
    }
    
    public void ChangeAmbienceVolume(float volume) {
        SoundFXManager.obj.PlayUISlider();
        SoundMixerManager.obj.SetAmbienceVolume(volume);
    }

    public void ShowKeyboardConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();

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

    void OnDestroy() {
        obj = null;
    }
}
