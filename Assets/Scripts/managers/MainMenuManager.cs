using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _caveRoom1;
    [SerializeField] private SceneField _titleScreen;

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
    private Color _backButtonColor;
    [SerializeField] private GameObject _musicSliderGameObject;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _ambienceSlider;

    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _keyboardConfigMenu;
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
        EventSystem.current.SetSelectedGameObject(_playButton);
        _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        _backButtonColor = _backButton.GetComponentInChildren<TextMeshProUGUI>().color;
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

        //Reset player properties
        Player.obj.SetHasCape(false);
        Player.obj.CanFallDash = false;
        Player.obj.CanForcePushJump = false;
        CollectibleManager.obj.ResetCollectibles();
        //TODO reset number of lives died

        SoundMixerManager.obj.SetMasterVolume(masterVolume);
        AsyncOperation loadCaveOperation = SceneManager.LoadSceneAsync(_caveRoom1, LoadSceneMode.Additive);
        while(!loadCaveOperation.isDone) {
            yield return null;
        }
        Scene caveRoom1 = SceneManager.GetSceneByName(_caveRoom1.SceneName);
        SceneManager.SetActiveScene(caveRoom1);
        
        Player.obj.SetCaveStartingCoordinates();
        CaveAvatar.obj.SetCaveStartingCoordinates();
        PlayerMovement.obj.DisablePlayerMovement();
        
        GameObject[] sceneGameObjects = caveRoom1.GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        cameraManager.ActivateMainCamera(PlayerMovement.PlayerDirection.NO_DIRECTION);

        GameObject levelSwitcherGameObject = sceneGameObjects.First(gameObject => gameObject.CompareTag("LevelSwitcher"));
        LevelSwitcher levelSwitcher = levelSwitcherGameObject.GetComponent<LevelSwitcher>();
        levelSwitcher.LoadNextRoom();

        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    private void StartFadeOut() {
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }

    public void OnOptionsButtonClicked() {
        SoundFXManager.obj.PlayUIConfirm();
        ShowOptionsMenu();
    }

    public void ShowOptionsMenu() {
        _musicSlider.value = SoundMixerManager.obj.GetMusicVolume();
        _soundFXSlider.value = SoundMixerManager.obj.GetSoundFXVolume();
        _ambienceSlider.value = SoundMixerManager.obj.GetAmbienceVolume();

        _titleMenu.SetActive(false);
        _keyboardConfigMenu.SetActive(false);
        HideControllerConfigMenu();
        _optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_musicSliderGameObject);

        //Reset color of options button from animation
        TextMeshProUGUI textMeshPro = _optionsButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
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

        //Reset color of keyboard config button from animation
        TextMeshProUGUI textMeshPro = _keyboardConfigButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
    }

    public void LeaveKeyboardConfigMenu() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        SoundFXManager.obj.PlayUIBack();
        ShowOptionsMenu();
        EventSystem.current.SetSelectedGameObject(_keyboardConfigButton.gameObject);

        //Reset color of back button from animation
        TextMeshProUGUI textMeshPro = _keyboardConfigMenuBackButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _backButtonColor;
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

        //Reset color of controller config button from animation
        TextMeshProUGUI textMeshPro = _controllerConfigButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
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

        //Reset color of back button from animation
        TextMeshProUGUI textMeshPro = _controllerConfigMenuBackButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _backButtonColor;
    }

    public void ShowTitleMenu() {
        SoundFXManager.obj.PlayUIBack();

        _optionsMenu.SetActive(false);
        _titleMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_optionsButton.gameObject);

        //Reset color of back button from animation
        TextMeshProUGUI textMeshPro = _backButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _backButtonColor;
    }

    public void ChangeMusicVolume(float volume) {
        SoundMixerManager.obj.SetMusicVolume(volume);
    }

    public void ChangeSoundFxVolume(float volume) {
        SoundMixerManager.obj.SetSoundFXVolume(volume);
    }    
    
    public void ChangeAmbienceVolume(float volume) {
        SoundMixerManager.obj.SetAmbienceVolume(volume);
    }

    public void OnNavigateBack() {
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

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }    
}
