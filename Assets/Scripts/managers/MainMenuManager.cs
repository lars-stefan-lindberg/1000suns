using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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
    [SerializeField] private GameObject _singlePlayerButton;
    [SerializeField] private GameObject _coopButton;
    [SerializeField] private GameObject _playCoopButton;
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

    [SerializeField] private GameObject _playerDeviceSetup;
    [SerializeField] private GameObject _characterSelection;
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

    [SerializeField] private TextMeshProUGUI _player1DeviceLabel;
    [SerializeField] private TextMeshProUGUI _player2DeviceLabel;
    [SerializeField] private GameObject _player2JoinXboxIcon;
    [SerializeField] private GameObject _player2JoinPSIcon;
    [SerializeField] private GameObject _player2JoinKeyboardText;
    [SerializeField] private GameObject _playerDeviceSetupConfirmButton;
    [SerializeField] private GameObject _characterSelectionWidgetPrefab;
    [SerializeField] private GameObject _confirmCancelButtonsPanel;
    private bool _hasPlayer2Joined;
    private InputDevice _player1InputDevice;

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
        InputDeviceListener.OnGamepadConnected += HandleGamepadConnected;
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
            //EventSystem.current.SetSelectedGameObject(_continueButton);
            EventSystem.current.SetSelectedGameObject(_playButton);
        }
    }

    private float _coopGameStartCounddownTime = 0.5f;
    private float _coopGameStartCounddownTimer = 0f;
    private bool _isGameStarted = false;
    void Update()
    {
        if(_characterSelection.activeSelf && !_isGameStarted) {
            if(_characterSelectionWidgets.All(widget => widget.IsReady))
            {
                _coopGameStartCounddownTimer += Time.deltaTime;
                if(_coopGameStartCounddownTimer >= _coopGameStartCounddownTime)
                {
                    StartCoopGame();
                    _isGameStarted = true;
                }
            } else {
                _coopGameStartCounddownTimer = 0f;
            }
        }
        if (!_playerDeviceSetup.activeSelf)
        {
            return;
        }

        if (_hasPlayer2Joined)
        {
            return;
        }

        var availableDevices = InputDeviceListener.obj.GetAvailableDevicesExcluding(_player1InputDevice);

        foreach (var info in availableDevices)
        {
            if (info.DeviceType == InputDeviceListener.JoinDeviceType.Keyboard)
            {
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    _hasPlayer2Joined = true;
                    SetupPlayer2InputDevice(info);
                    return;
                }
            }
            else
            {
                var gamepad = info.Device as Gamepad;

                if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame)
                {
                    _hasPlayer2Joined = true;
                    SetupPlayer2InputDevice(info);
                    return;
                }
            }
        }
    }

    void OnDestroy() {
        InputDeviceListener.OnInputDeviceStream -= HandleInputDeviceChanged;
        InputDeviceListener.OnGamepadConnected -= HandleGamepadConnected;
        obj = null;
    }

    public void OnPlayButtonClicked() {
        // bool hasValidSave = SaveManager.obj.HasValidSave(); 
        // if (!hasValidSave) {
        //     StartGame();
        // } else {
        //     isNavigatingToMenu = true;
        //     SoundFXManager.obj.PlayUIConfirm();

        //     ShowConfirmNewGameMenu();
        //     EventSystem.current.SetSelectedGameObject(_confirmNewGameButton.gameObject);
        // }
        _playButton.SetActive(false);
        _optionsButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);

        _singlePlayerButton.SetActive(true);
        _coopButton.SetActive(true);

        SoundFXManager.obj.PlayUIConfirm();
        EventSystem.current.SetSelectedGameObject(_singlePlayerButton);
    }

    public void OnPlayCoopButtonClicked() {
        StartCoopGame();
    }

    public void StartGame() {
        _playButton.GetComponent<Button>().interactable = false;
        StartCoroutine(StartGameCoroutine());
    }

    public void StartCoopGame() {
        _playCoopButton.GetComponent<Button>().interactable = false;
        StartCoroutine(StartCoopGameCoroutine());
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
        LevelManager.obj.ResetLevels();

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

    private IEnumerator StartCoopGameCoroutine() {
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
        Player.obj.SetHasCape(true);
        Player.obj.gameObject.SetActive(false);
        ShadowTwinPlayer.obj.SetHasCrown(true);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        PlayerPowersManager.obj.ResetGameEvents();
        PlayerPowersManager.obj.CanSeparate = true;
        PlayerPowersManager.obj.CanSwitchBetweenTwinsMerged = true;
        CollectibleManager.obj.ResetCollectibles();
        PlayerStatsManager.obj.numberOfDeaths = 0;
        LevelManager.obj.ResetLevels();
        PlayerManager.obj.IsSeparated = true;
        if(LobbyManager.obj.GetPlayerSlots().Count > 1) {
            PlayerManager.obj.IsCoopActive = true;
        }

        SoundMixerManager.obj.SetMasterVolume(masterVolume);

        AsyncOperation loadC1_1Operation = SceneManager.LoadSceneAsync("C1-1", LoadSceneMode.Additive);
        while(!loadC1_1Operation.isDone) {
            yield return null;
        }
        Scene c1_1Scene = SceneManager.GetSceneByName("C1-1");
        SceneManager.SetActiveScene(c1_1Scene);

        GameObject[] sceneGameObjects = c1_1Scene.GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        cameraManager.ActivateMainCamera();

        GameObject levelSwitcherGameObject = sceneGameObjects.First(gameObject => gameObject.CompareTag("LevelSwitcher"));
        LevelSwitcher levelSwitcher = levelSwitcherGameObject.GetComponent<LevelSwitcher>();
        levelSwitcher.LoadNextRoom();
        
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

    public void OnSinglePlayerButtonClicked() {
        isNavigatingToMenu = true;
        SoundFXManager.obj.PlayUIConfirm();
        
        StartCoopGame();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnCoopButtonClicked() {
        isNavigatingToMenu = true;
        SoundFXManager.obj.PlayUIConfirm();
        
        ShowPlayerDeviceSetupMenu();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPlayerDeviceSetupConfirmButtonClicked() {
        isNavigatingToMenu = true;
        SoundFXManager.obj.PlayUIConfirm();

        ShowCharacterSelectionMenu();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnConfirmNewGameButtonClicked() {
        _confirmNewGameButton.interactable = false;
        SaveManager.obj.DeleteSave();
        StartCoroutine(StartGameCoroutine());
    }

    private List<CharacterSelectionWidget> _characterSelectionWidgets = new List<CharacterSelectionWidget>();
    public void ShowCharacterSelectionMenu() {
        _playerDeviceSetup.SetActive(false);
        UpdateCancelConfirmUI(InputDeviceListener.Device.None);
        _characterSelection.SetActive(true);
        LobbyManager.obj.IsJoiningPlayers = false;
        LobbyManager.obj.IsSelectingCharacters = true;

        //Instantiate character selection widget
        PlayerSlot playerSlot = LobbyManager.obj.GetPlayerSlots()[0];
        PlayerInput player1Input = PlayerInput.Instantiate(
            _characterSelectionWidgetPrefab,
            controlScheme: null,
            pairWithDevice: playerSlot.device
        );
        _characterSelectionWidgets.Add(player1Input.gameObject.GetComponent<CharacterSelectionWidget>());
        player1Input.transform.SetParent(_characterSelection.transform, worldPositionStays: false);

        CharacterSelectionWidget _widget1 = player1Input.gameObject.GetComponent<CharacterSelectionWidget>();
        _widget1.playerIndex = playerSlot.slotIndex;
        _widget1.SetPlayerSlot(playerSlot);
        RectTransform rectTransform1 = _widget1.GetRectTransform();
        rectTransform1.anchoredPosition = new Vector2(rectTransform1.anchoredPosition.x, 165);

        //Instantiate character selection widget
        PlayerSlot playerSlot2 = LobbyManager.obj.GetPlayerSlots()[1];
        PlayerInput player2Input = PlayerInput.Instantiate(
            _characterSelectionWidgetPrefab,
            controlScheme: null,
            pairWithDevice: playerSlot2.device
        );
        _characterSelectionWidgets.Add(player2Input.gameObject.GetComponent<CharacterSelectionWidget>());
        player2Input.transform.SetParent(_characterSelection.transform, worldPositionStays: false);

        CharacterSelectionWidget _widget2 = player2Input.gameObject.GetComponent<CharacterSelectionWidget>();
        _widget2.playerIndex = playerSlot2.slotIndex;
        _widget2.SetPlayerSlot(playerSlot2);
        RectTransform rectTransform2 = _widget2.GetRectTransform();
        rectTransform2.anchoredPosition = new Vector2(rectTransform2.anchoredPosition.x, -139);
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

    public void ShowPlayerDeviceSetupMenu() {
        _titleMenu.SetActive(false);

        _playerDeviceSetup.SetActive(true);
        _hasPlayer2Joined = false;
        LobbyManager.obj.IsJoiningPlayers = true;
        _playerDeviceSetupConfirmButton.GetComponent<Button>().interactable = false;
        _playerDeviceSetupConfirmButton.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;

        if (InputDeviceListener.obj.GetCurrentInputDevice() == InputDeviceListener.Device.Gamepad)
        {
            _player1InputDevice = Gamepad.current;
            _player1DeviceLabel.text = "Gamepad";
            LobbyManager.obj.AddPlayerSlot(new PlayerSlot { slotIndex = 0, device = Gamepad.current });
        }
        else
        {
            _player1InputDevice = Keyboard.current;
            _player1DeviceLabel.text = "Keyboard";
            LobbyManager.obj.AddPlayerSlot(new PlayerSlot { slotIndex = 0, device = Keyboard.current });
        }
        UpdatePlayerDeviceSetupPlayer2UI();
    }

    private void UpdatePlayerDeviceSetupPlayer2UI()
    {
        int numberOfAvailableDevices = InputDeviceListener.obj.GetAvailableDeviceCount();
        if(numberOfAvailableDevices == 1) {
            _player2DeviceLabel.text = "Connect device";
            _player2JoinXboxIcon.SetActive(false);
            _player2JoinPSIcon.SetActive(false);
            _player2JoinKeyboardText.SetActive(false);
        } else {
            _player2DeviceLabel.text = "Join";
            _player2JoinXboxIcon.SetActive(false);
            _player2JoinPSIcon.SetActive(false);
            _player2JoinKeyboardText.SetActive(false);
            var avialableDevices = InputDeviceListener.obj.GetAvailableNonActiveDevices();

            foreach (var info in avialableDevices)
            {
                switch (info.DeviceType)
                {
                    case InputDeviceListener.JoinDeviceType.Keyboard:
                        _player2JoinKeyboardText.SetActive(true);
                        break;
                    case InputDeviceListener.JoinDeviceType.XboxGamepad:
                        _player2JoinXboxIcon.SetActive(true);
                        break;
                    case InputDeviceListener.JoinDeviceType.PlayStationGamepad:
                        _player2JoinPSIcon.SetActive(true);
                        break;
                    case InputDeviceListener.JoinDeviceType.OtherGamepad:
                        _player2JoinXboxIcon.SetActive(true);
                        break;
                }
            }
        }
    }

    private void HandleGamepadConnected() {
        if(_playerDeviceSetup.activeSelf) {
            UpdatePlayerDeviceSetupPlayer2UI();
        }
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
        } else if(_titleMenu.activeSelf && _singlePlayerButton.activeSelf) {
            LeaveToMainMenu();
        } else if(_playerDeviceSetup.activeSelf) {
            LeaveToPlayModeMenu();
        } else if(_characterSelection.activeSelf) {
            LeaveToPlayerDeviceSetup();
        }
    }

    private void LeaveToPlayerDeviceSetup() {
        _characterSelection.SetActive(false);
        _characterSelectionWidgets.ForEach((widget) => {
            Destroy(widget.gameObject);
        });
        _characterSelectionWidgets.Clear();
        LobbyManager.obj.IsJoiningPlayers = true;
        _playerDeviceSetup.SetActive(true);
        UpdateCancelConfirmUI(InputDeviceListener.obj.GetCurrentInputDevice());
        EventSystem.current.SetSelectedGameObject(_playerDeviceSetupConfirmButton);
    }

    public void LeaveToMainMenu() {
        SoundFXManager.obj.PlayUIBack();
        _singlePlayerButton.SetActive(false);
        _coopButton.SetActive(false);
        _playButton.SetActive(true);
        _optionsButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_playButton);
    }

    public void LeaveToPlayModeMenu() {
        LobbyManager.obj.ClearPlayerSlots();
        LobbyManager.obj.IsJoiningPlayers = false;
        SoundFXManager.obj.PlayUIBack();
        _playerDeviceSetup.SetActive(false);
        _titleMenu.SetActive(true);
        _singlePlayerButton.SetActive(true);
        _coopButton.SetActive(true);
        _playButton.SetActive(false);
        _optionsButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(_coopButton);
    }

    private void SetupPlayer2InputDevice(InputDeviceListener.AvailableInputDeviceInfo deviceInfo)
    {
        SoundFXManager.obj.PlayUIConfirm();
        LobbyManager.obj.AddPlayerSlot(new PlayerSlot { slotIndex = 1, device = deviceInfo.Device });
        //If device is keyboard show text "Keyboard", else show "Gamepad"
        _player2DeviceLabel.text = deviceInfo.Device is Keyboard ? "Keyboard" : "Gamepad";
        _player2JoinXboxIcon.SetActive(false);
        _player2JoinPSIcon.SetActive(false);
        _player2JoinKeyboardText.SetActive(false);
        _playerDeviceSetupConfirmButton.GetComponent<Button>().interactable = true;
        _playerDeviceSetupConfirmButton.GetComponentInChildren<TextMeshProUGUI>().color = _optionsButtonColor;
        EventSystem.current.SetSelectedGameObject(_playerDeviceSetupConfirmButton);
    }

    public void HandleInputDeviceChanged(InputDeviceListener.Device device) {
        UpdateCancelConfirmUI(device);
    }

    public void UpdateCancelConfirmUI(InputDeviceListener.Device device) {
        if(device == InputDeviceListener.Device.Gamepad) {
            _confirmCancelButtonsPanel.SetActive(true);
            _gamepadGeneralConfirmIcon.gameObject.SetActive(true);
            _gamepadGeneralCancelIcon.gameObject.SetActive(true);
            _keyboardGeneralCancelText.gameObject.SetActive(false);
            _keyboardGeneralConfirmText.gameObject.SetActive(false);
            Sprite confirmButtonSprite = GamepadIconManager.obj.GetIcon(confirmActionReference.action);
            _gamepadGeneralConfirmIcon.sprite = confirmButtonSprite;
            Sprite cancelButtonSprite = GamepadIconManager.obj.GetIcon(cancelActionReference.action);
            _gamepadGeneralCancelIcon.sprite = cancelButtonSprite;
        } else if(device == InputDeviceListener.Device.None) {
            _confirmCancelButtonsPanel.SetActive(false);
        } else {
            _confirmCancelButtonsPanel.SetActive(true);
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
