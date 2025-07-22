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
    [SerializeField] private GameObject _fadeOutCanvas;

    [SerializeField] private Image _fadeOutImage;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;

    [SerializeField] private Color _fadeOutStartColor;

    public bool IsFadingOut { get; private set; }

    [SerializeField] private GameObject _playButton;
    [SerializeField] private Button _optionsButton;
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

        Canvas titleScreenCanvas = _titleScreenCanvas.GetComponent<Canvas>();
        titleScreenCanvas.worldCamera = Camera.main;
        titleScreenCanvas.sortingLayerName = "UI";

        Canvas fadeOutCanvas = _fadeOutCanvas.GetComponent<Canvas>();
        fadeOutCanvas.worldCamera = Camera.main;
        fadeOutCanvas.sortingLayerName = "UI";

        EventSystem.current.SetSelectedGameObject(_playButton);
        _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        MusicManager.obj.PlayTitleSong();
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));

        InputDeviceListener.OnInputDeviceStream += HandleInputDeviceChanged;
        HandleInputDeviceChanged(InputDeviceListener.obj.GetCurrentInputDevice());
    }

    void OnDestroy() {
        InputDeviceListener.OnInputDeviceStream -= HandleInputDeviceChanged;
        obj = null;
    }

    void Update() {
        if(IsFadingOut) {
            if(_fadeOutImage.color.a < 1f) {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            } else {
                IsFadingOut = false;
            }
        }
    }

    public void StartGame() {
        _playButton.GetComponent<Button>().interactable = false;
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        SoundFXManager.obj.PlayUIPlay();

        float masterVolume = SoundMixerManager.obj.GetMasterVolume();

        StartCoroutine(SoundMixerManager.obj.StartMasterFade(3f, 0.001f));
        StartFadeOut();
        while(IsFadingOut)
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
        PlayerMovement.obj.DisablePlayerMovement();

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

    private void StartFadeOut() {
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }

    public void OnOptionsButtonClicked() {
        isNavigatingToMenu = true;
        SoundFXManager.obj.PlayUIConfirm();
        
        ShowOptionsMenu();
        EventSystem.current.SetSelectedGameObject(_musicSliderGameObject);
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
