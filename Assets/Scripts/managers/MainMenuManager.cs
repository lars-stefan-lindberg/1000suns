using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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
    private Color _backButtonColor;
    [SerializeField] private GameObject _musicSliderGameObject;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _ambienceSlider;

    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _keyboardConfigMenu;
    [SerializeField] private GameObject _controllerConfigMenu;
    [SerializeField] private GameObject _titleMenu;

    [SerializeField] private TextMeshProUGUI _keyboardConfigInstructionsConfirmActionKeyText;
    public InputActionAsset actions;
    public InputActionReference confirmActionReference;
    private string confirmActionKeyboardDisplayString;

    void Awake() {
        EventSystem.current.SetSelectedGameObject(_playButton);
        _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        _backButtonColor = _backButton.GetComponentInChildren<TextMeshProUGUI>().color;
        MusicManager.obj.PlayTitleSong();
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
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
        _controllerConfigMenu.SetActive(false);
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
        EventSystem.current.SetSelectedGameObject(FindFirstObjectByType<Button>().gameObject);

        //Reset color of keyboard config button from animation
        TextMeshProUGUI textMeshPro = _keyboardConfigButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
    }

    public void LeaveKeyboardConfigMenu() {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);

        SoundFXManager.obj.PlayUIBack();
        ShowOptionsMenu();
    }

    public void KeyboardRebindEventHandler() {
        _keyboardConfigInstructionsConfirmActionKeyText.text = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
    }

    public void ShowControllerConfigMenu() {
        SoundFXManager.obj.PlayUIConfirm();

        _optionsMenu.SetActive(false);
        _controllerConfigMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(FindFirstObjectByType<Button>().gameObject);

        //Reset color of controller config button from animation
        TextMeshProUGUI textMeshPro = _controllerConfigButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
    }

    public void ShowTitleMenu() {
        SoundFXManager.obj.PlayUIBack();

        _optionsMenu.SetActive(false);
        _titleMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_playButton);

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
}
