using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager obj;
    public bool isNavigatingToMenu = true;
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _introScene;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private GameObject _titleScreenCanvas;

    public bool IsFadingOut { get; private set; }

    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _confirmNewGameButton;
    [SerializeField] private Button _declineNewGameButton;
    [SerializeField] private Button _keyboardConfigButton;
    [SerializeField] private Button _controllerConfigButton;
    private Color _optionsButtonColor;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _controllerConfigMenuBackButton;
    [SerializeField] private Button _keyboardConfigMenuBackButton;
    [SerializeField] private GameObject _musicSliderGameObject;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _ambienceSlider;

    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _keyboardConfigMenu;
    [SerializeField] private GameObject _confirmNewGameMenu;
    [SerializeField] private AutoScrollRect _keyboardConfigAutoScroll;
    [SerializeField] private Button _firstKeyboardMenuButton;
    [SerializeField] private GameObject _controllerConfigMenu;
    [SerializeField] private GameObject _controllerConfigMenuShowConfig;
    [SerializeField] private GameObject _controllerConfigMenuShowAttachController;
    [SerializeField] private Button _firstControllerMenuButton;
    [SerializeField] private GameObject _titleMenu;

    [SerializeField] private TextMeshProUGUI _keyboardConfigInstructionsConfirmActionKeyText;
    [SerializeField] private Image _gamepadConfigInstructionsConfirmActionKeyIcon;
    [SerializeField] private Image _gamepadConfigInstructionsResetButtonActionKeyIcon;
    [SerializeField] private Sprite _gamepadConfirmIcon;
    public InputActionAsset actions;
    public InputActionReference confirmActionReference;
    public InputActionReference cancelActionReference;
    public InputActionReference resetButtonActionReference;
    private string confirmActionKeyboardDisplayString;

    [SerializeField] private Image _gamepadGeneralConfirmIcon;
    [SerializeField] private Image _gamepadGeneralCancelIcon;
    [SerializeField] private TextMeshProUGUI _keyboardGeneralCancelText;
    [SerializeField] private TextMeshProUGUI _keyboardGeneralConfirmText;

    void Awake() {
        obj = this;
    }

    void Start() {
        Canvas titleScreenCanvas = _titleScreenCanvas.GetComponent<Canvas>();
        titleScreenCanvas.worldCamera = Camera.main;
        titleScreenCanvas.sortingLayerName = "UI";

        _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        MusicManager.obj.PlayTitleSong();
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));

        InputDeviceListener.OnInputDeviceStream += HandleInputDeviceChanged;
        HandleInputDeviceChanged(InputDeviceListener.obj.GetCurrentInputDevice());

        bool hasValidSave = SaveManager.obj.HasValidSave(); 
        if (!hasValidSave) {
            EventSystem.current.SetSelectedGameObject(_playButton);

            Button playButton = _playButton.GetComponent<Button>(); 
            Navigation playButtonNewNav = playButton.navigation;
            playButtonNewNav.selectOnUp = _exitButton;
            playButton.navigation = playButtonNewNav;

            Navigation exitButtonNewNav = _exitButton.navigation;
            exitButtonNewNav.selectOnDown = playButton;
            _exitButton.navigation = exitButtonNewNav;

            Button continueButton = _continueButton.GetComponent<Button>();
            continueButton.interactable = false;
            TextMeshProUGUI continueButtonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            continueButtonText.color = new Color(0.65f, 0.65f, 0.65f, 1f);
        } else {
            EventSystem.current.SetSelectedGameObject(_continueButton);
        }
    }

    void OnDestroy() {
        InputDeviceListener.OnInputDeviceStream -= HandleInputDeviceChanged;
        obj = null;
    }

    public void OnPlayButtonClicked() {
        bool hasValidSave = SaveManager.obj.HasValidSave(); 
        if (!hasValidSave) {
            StartGame();
        } else {
            isNavigatingToMenu = true;
            SoundFXManager.obj.PlayUIConfirm();

            ShowConfirmNewGameMenu();
            EventSystem.current.SetSelectedGameObject(_confirmNewGameButton.gameObject);
        }
    }

    public void StartGame() {
        _playButton.GetComponent<Button>().interactable = false;
        StartCoroutine(StartGameCoroutine());
    }

    [ContextMenu("Continue Game")]
    public void ContinueGame() {
        _continueButton.GetComponent<Button>().interactable = false;
        StartCoroutine(ContinueGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        SoundFXManager.obj.PlayUIPlay();

        float masterVolume = SoundMixerManager.obj.GetMasterVolume();

        StartCoroutine(SoundMixerManager.obj.StartMasterFade(3f, 0.001f));
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;
        while(SoundMixerManager.obj.GetMasterVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();

        AsyncOperation loadPersistentGameplayOperation = SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        while(!loadPersistentGameplayOperation.isDone) {
            yield return null;
        }
        GameEventManager.obj.IsPauseAllowed = false;

        //Reset player properties
        Player.obj.SetHasCape(false);
        PlayerPowersManager.obj.ResetGameEvents();
        CollectibleManager.obj.ResetCollectibles();
        PlayerStatsManager.obj.numberOfDeaths = 0;
        Player.obj.gameObject.SetActive(false);

        SoundMixerManager.obj.SetMasterVolume(masterVolume);
        AsyncOperation loadIntroSceneOperation = SceneManager.LoadSceneAsync(_introScene, LoadSceneMode.Additive);
        while(!loadIntroSceneOperation.isDone) {
            yield return null;
        }
        Scene introScene = SceneManager.GetSceneByName(_introScene.SceneName);
        SceneManager.SetActiveScene(introScene);

        GameObject[] sceneGameObjects = introScene.GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        cameraManager.ActivateMainCamera();
        
        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    private IEnumerator ContinueGameCoroutine() {
        SoundFXManager.obj.PlayUIPlay();

        float masterVolume = SoundMixerManager.obj.GetMasterVolume();

        StartCoroutine(SoundMixerManager.obj.StartMasterFade(3f, 0.001f));
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;
        while(SoundMixerManager.obj.GetMasterVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();
        
        AsyncOperation loadPersistentGameplayOperation = SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        while(!loadPersistentGameplayOperation.isDone) {
            yield return null;
        }
        GameEventManager.obj.IsPauseAllowed = false;

        SaveData saveData = SaveManager.obj.LoadGame();
        if(saveData == null) {
            Debug.LogWarning("No save data found. Starting new game.");
            StartCoroutine(StartGameCoroutine());
            yield break;
        }

        yield return new WaitForSeconds(1f);
        SoundMixerManager.obj.SetMasterVolume(masterVolume);
        LevelManager.obj.LoadSceneDelayed(saveData.levelId);

        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    public void OnOptionsButtonClicked() {
        isNavigatingToMenu = true;
        SoundFXManager.obj.PlayUIConfirm();
        
        ShowOptionsMenu();
        EventSystem.current.SetSelectedGameObject(_musicSliderGameObject);
    }

    public void OnConfirmNewGameButtonClicked() {
        _confirmNewGameButton.interactable = false;
        SaveManager.obj.DeleteSave();
        StartCoroutine(StartGameCoroutine());
    }

    public void ShowConfirmNewGameMenu() {
        _titleMenu.SetActive(false);
        _confirmNewGameMenu.SetActive(true);
    }

    public void LeaveConfirmNewGameMenu() {
        _confirmNewGameMenu.SetActive(false);
        SoundFXManager.obj.PlayUIBack();
        EventSystem.current.SetSelectedGameObject(null);

        _titleMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_playButton);
    }

    public void ShowOptionsMenu() {
        _musicSlider.value = SoundMixerManager.obj.GetMusicVolume();
        _soundFXSlider.value = SoundMixerManager.obj.GetSoundFXVolume();
        _ambienceSlider.value = SoundMixerManager.obj.GetAmbienceVolume();

        _titleMenu.SetActive(false);
        _keyboardConfigMenu.SetActive(false);
        HideControllerConfigMenu();
        _optionsMenu.SetActive(true);
    }

    public void ShowKeyboardConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();

        _optionsMenu.SetActive(false);

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

        ShowOptionsMenu();
        EventSystem.current.SetSelectedGameObject(_keyboardConfigButton.gameObject);
    }

    public void KeyboardConfirmRebindEventHandler() {
        string confirmText = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
        _keyboardConfigInstructionsConfirmActionKeyText.text = confirmText;
        _keyboardGeneralConfirmText.text = confirmText;
    }

    public void KeyboardCancelRebindEventHandler() {
        _keyboardGeneralCancelText.text = cancelActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));;
    }

    public void GamepadConfirmRebindEventHandler() {
        Sprite sprite = GamepadIconManager.obj.GetIcon(confirmActionReference.action);
        _gamepadConfigInstructionsConfirmActionKeyIcon.sprite = sprite;    
        _gamepadGeneralConfirmIcon.sprite = sprite;    
    }

    public void GamepadCancelRebindEventHandler() {
        Sprite sprite = GamepadIconManager.obj.GetIcon(cancelActionReference.action);
        _gamepadGeneralCancelIcon.sprite = sprite;    
    }

    public void ShowControllerConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();

        EventSystem.current.SetSelectedGameObject(null); //Make sure we unselect the controller config menu option. If we show attach controller screen, no other UI element will be selected, and going back won't select the controller config menu option
        _optionsMenu.SetActive(false);
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

    private void HideControllerConfigMenu() {
        _controllerConfigMenu.SetActive(false);
        _controllerConfigMenuShowAttachController.SetActive(false);
        _controllerConfigMenuShowConfig.SetActive(false);
    }

    public void LeaveControllerConfigMenu() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        SoundFXManager.obj.PlayUIBack();
        ShowOptionsMenu();

        EventSystem.current.SetSelectedGameObject(_controllerConfigButton.gameObject);
    }

    public void ShowTitleMenu() {
        SoundFXManager.obj.PlayUIBack();

        _optionsMenu.SetActive(false);
        _titleMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_optionsButton.gameObject);
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

    public void OnNavigateBack() {
        isNavigatingToMenu = true;

        if(_optionsMenu.activeSelf) {
            ShowTitleMenu();
        } else if(_keyboardConfigMenu.activeSelf) {
            LeaveKeyboardConfigMenu();
        } else if(_controllerConfigMenu.activeSelf) {
            LeaveControllerConfigMenu();
        } else if(_confirmNewGameMenu.activeSelf) {
            LeaveConfirmNewGameMenu();
        }
    }

    public void HandleInputDeviceChanged(InputDeviceListener.Device device) {
        UpdateCancelConfirmUI(device);
    }

    public void UpdateCancelConfirmUI(InputDeviceListener.Device device) {
        if(device == InputDeviceListener.Device.Gamepad) {
            _gamepadGeneralConfirmIcon.gameObject.SetActive(true);
            _gamepadGeneralCancelIcon.gameObject.SetActive(true);
            _keyboardGeneralCancelText.gameObject.SetActive(false);
            _keyboardGeneralConfirmText.gameObject.SetActive(false);
            Sprite confirmButtonSprite = GamepadIconManager.obj.GetIcon(confirmActionReference.action);
            _gamepadGeneralConfirmIcon.sprite = confirmButtonSprite;
            Sprite cancelButtonSprite = GamepadIconManager.obj.GetIcon(cancelActionReference.action);
            _gamepadGeneralCancelIcon.sprite = cancelButtonSprite;
        } else {
            _gamepadGeneralConfirmIcon.gameObject.SetActive(false);
            _gamepadGeneralCancelIcon.gameObject.SetActive(false);
            _keyboardGeneralCancelText.gameObject.SetActive(true);
            _keyboardGeneralConfirmText.gameObject.SetActive(true);
            _keyboardGeneralConfirmText.text = confirmActionKeyboardDisplayString;
            _keyboardGeneralCancelText.text = cancelActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
        }
    }

    public Color GetMainButtonTextColor() {
        return _optionsButtonColor;
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }    
}
