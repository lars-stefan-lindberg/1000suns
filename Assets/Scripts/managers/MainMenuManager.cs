using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager obj;
    [SerializeField] private CanvasGroup _mainMenuScreen;
    [SerializeField] private CanvasGroup _mainMenuVisualsScreen;
    [SerializeField] private GameObject _startGameButton;
    [SerializeField] private GameObject _optionsButton;
    [SerializeField] private SelectSaveFileScreen _selectSaveFileScreen;
    [SerializeField] private OptionsScreen _optionsScreen;
    [SerializeField] private GameObject _optionsMenuGameOptionsButton;
    [SerializeField] private GameObject _optionsMenuAudioButton;
    [SerializeField] private GameObject _optionsMenuControllerButton;
    [SerializeField] private GameObject _optionsMenuKeyboardButton;
    [SerializeField] private GameOptionsScreen _gameOptionsScreen;
    [SerializeField] private AudioScreen _audioScreen;
    [SerializeField] private ControllerScreen _controllerScreen;
    [SerializeField] private KeyboardScreen _keyboardScreen;
    [SerializeField] private MusicTrack _titleScreenMusic;
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _introScene;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private SceneField _firstCaveBackground;
    [SerializeField] private SceneField _firstCaveSurfaces;
    [SerializeField] private GameObject _titleScreenCanvas;
    [SerializeField] private GameObject _particlesCanvas;
    [SerializeField] private GameObject _titleTextCanvas;
    [SerializeField] private GameObject _lightsCanvas;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _glitchKeyStudios;
    [SerializeField] private GameObject _fmod;
    [SerializeField] private InputActionReference _cancelActionReference;
 
    private Sequence _menuTransitionSequence;
    private bool _isTransitioning = false;
    private Stack<UIScreen> screenStack = new();
    private GameObject _mainMenuBackSelectable;

    void Awake() {
        obj = this;
    }

    void Start() {
        //TODO set all canvases camera and sorting layer name. Has to be done since the camera is in another scene
        Canvas titleScreenCanvas = _titleScreenCanvas.GetComponent<Canvas>();
        SetCanvasCamera(titleScreenCanvas);

        Canvas particlesCanvas = _particlesCanvas.GetComponent<Canvas>();
        SetCanvasCamera(particlesCanvas);

        Canvas titleTextCanvas = _titleTextCanvas.GetComponent<Canvas>();
        SetCanvasCamera(titleTextCanvas);
        
        Canvas lightsCanvas = _lightsCanvas.GetComponent<Canvas>();
        SetCanvasCamera(lightsCanvas);

        Canvas optionsScreenCanvas = _optionsScreen.GetComponent<Canvas>();
        SetCanvasCamera(optionsScreenCanvas);

        Canvas audioScreenCanvas = _audioScreen.GetComponent<Canvas>();
        SetCanvasCamera(audioScreenCanvas);

        Canvas gameConfigCanvas = _gameOptionsScreen.GetComponent<Canvas>();
        SetCanvasCamera(gameConfigCanvas);

        Canvas controllerConfigCanvas = _controllerScreen.GetComponent<Canvas>();
        SetCanvasCamera(controllerConfigCanvas);

        Canvas keyboardConfigCanvas = _keyboardScreen.GetComponent<Canvas>();
        SetCanvasCamera(keyboardConfigCanvas);

        Canvas selectSaveFileCanvas = _selectSaveFileScreen.GetComponent<Canvas>();
        SetCanvasCamera(selectSaveFileCanvas);

        SceneFadeManager.obj.SetFadedOutState();
        //SceneFadeManager.obj.SetFadedInState();

        StartCoroutine(StartSequence());

        //EventSystem.current.SetSelectedGameObject(_playButton);
        EventSystem.current.SetSelectedGameObject(_playButton);

        MusicManager.obj.Play(_titleScreenMusic);


        _cancelActionReference.action.performed += OnCancel;

        // _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        
        // var rebinds = PlayerPrefs.GetString("rebinds");
        // if (!string.IsNullOrEmpty(rebinds))
        //     actions.LoadBindingOverridesFromJson(rebinds);
        // confirmActionKeyboardDisplayString = confirmActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));

        // InputDeviceListener.OnInputDeviceStream += HandleInputDeviceChanged;
        // InputDeviceListener.OnGamepadConnected += HandleGamepadConnected;
        // HandleInputDeviceChanged(InputDeviceListener.obj.GetCurrentInputDevice());

        // bool hasValidSave = SaveManager.obj.HasValidSave(); 
        // if (!hasValidSave) {
        //     EventSystem.current.SetSelectedGameObject(_playButton);

        //     Button playButton = _playButton.GetComponent<Button>(); 
        //     Navigation playButtonNewNav = playButton.navigation;
        //     playButtonNewNav.selectOnUp = _exitButton;
        //     playButton.navigation = playButtonNewNav;

        //     Navigation exitButtonNewNav = _exitButton.navigation;
        //     exitButtonNewNav.selectOnDown = playButton;
        //     _exitButton.navigation = exitButtonNewNav;

        //     Button continueButton = _continueButton.GetComponent<Button>();
        //     continueButton.interactable = false;
        //     TextMeshProUGUI continueButtonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        //     continueButtonText.color = new Color(0.65f, 0.65f, 0.65f, 1f);
        // } else {
        //     EventSystem.current.SetSelectedGameObject(_continueButton);
        // }
        // SceneFadeManager.obj.StartFadeIn();
    }

    private void SetCanvasCamera(Canvas canvas) {
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
    }

    private IEnumerator StartSequence() {
        SceneFadeManager.obj.StartFadeIn(1f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;
        // _glitchKeyStudios.SetActive(true);
        // SceneFadeManager.obj.StartFadeIn(0.5f);
        // yield return new WaitForSeconds(3f);
        // SceneFadeManager.obj.StartFadeOut(1f);
        // while(SceneFadeManager.obj.IsFadingOut)
        //     yield return null;
        // _glitchKeyStudios.SetActive(false);

        // _fmod.SetActive(true);
        // SceneFadeManager.obj.StartFadeIn(1f);
        // yield return new WaitForSeconds(3f);
        // SceneFadeManager.obj.StartFadeOut(1f);
        // while(SceneFadeManager.obj.IsFadingOut)
        //     yield return null;
        // _fmod.SetActive(false);

        //_title.SetActive(true);
        //SceneFadeManager.obj.StartFadeIn(1f);
        // MusicManager.obj.Play(_titleScreenMusic);

        yield return null;
    }

    void OnDestroy() {
        _cancelActionReference.action.performed -= OnCancel;
        _menuTransitionSequence = null;
        obj = null;
    }

    public void OnNewGameButtonClicked() {
        StartGame();

        //Co-op specific code:
        // _playButton.SetActive(false);
        // _optionsButton.gameObject.SetActive(false);
        // _exitButton.gameObject.SetActive(false);

        // _singlePlayerButton.SetActive(true);
        // _coopButton.SetActive(true);

        // SoundFXManager.obj.Play2D(_uiSoundLibrary.select);
        // EventSystem.current.SetSelectedGameObject(_singlePlayerButton);
    }

    public void StartGame() {
        //_playButton.GetComponent<Button>().interactable = false;
        StartCoroutine(StartGameCoroutine());
    }

    public void ContinueGame() {
        StartCoroutine(ContinueGameCoroutine());
    }

    private IEnumerator StartGameCoroutine() {
        UISoundPlayer.obj.PlayPlayGame();


        MusicManager.obj.Stop();

        SceneFadeManager.obj.StartFadeOut(0.5f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        yield return new WaitForSeconds(1f);
        _selectSaveFileScreen.Hide();

        AsyncOperation loadPersistentGameplayOperation = SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        while(!loadPersistentGameplayOperation.isDone) {
            yield return null;
        }
        GameManager.obj.IsPauseAllowed = false;

        //Reset game state
        Player.obj.SetAnimatorLayerAndHasCape(false);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(false);
        PlayerPowersManager.obj.ResetGameEvents();
        CollectibleManager.obj.ResetCollectibles();
        PlayerStatsManager.obj.numberOfDeaths = 0;
        Player.obj.gameObject.SetActive(false);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        PlayerManager.obj.IsSeparated = false;
        PlayerManager.obj.IsCoopActive = false;
        LevelManager.obj.ResetLevels();
        GameManager.obj.SetCaveTimeline(new CaveTimeline(CaveTimelineId.Id.Eli));

        // AsyncOperation loadIntroSceneOperation = SceneManager.LoadSceneAsync(_introScene, LoadSceneMode.Additive);
        // while(!loadIntroSceneOperation.isDone) {
        //     yield return null;
        // }
        // Scene introScene = SceneManager.GetSceneByName(_introScene.SceneName);
        // SceneManager.SetActiveScene(introScene);

        // GameObject[] sceneGameObjects = introScene.GetRootGameObjects();
        // GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        // CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        // cameraManager.ActivateMainCamera();

        AsyncOperation loadFirstCaveRoomOperation = SceneManager.LoadSceneAsync("Cave-1", LoadSceneMode.Additive);
        while(!loadFirstCaveRoomOperation.isDone) {
            yield return null;
        }
        Scene firstScene = SceneManager.GetSceneByName("Cave-1");
        SceneManager.SetActiveScene(firstScene);

        //Load background
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_firstCaveBackground));
        //Load surfaces
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstCaveSurfaces));

        LevelManager.obj.LoadAdjacentRooms(firstScene);
        
        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    private IEnumerator ContinueGameCoroutine() {
        UISoundPlayer.obj.PlayPlayGame();

        MusicManager.obj.Stop();

        SceneFadeManager.obj.StartFadeOut(0.5f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        yield return new WaitForSeconds(1f);
        _selectSaveFileScreen.Hide();
        
        AsyncOperation loadPersistentGameplayOperation = SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        while(!loadPersistentGameplayOperation.isDone) {
            yield return null;
        }
        GameManager.obj.IsPauseAllowed = false;

        Player.obj.gameObject.SetActive(false);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        PlayerManager.obj.IsSeparated = false;
        PlayerManager.obj.IsCoopActive = false;

        int activeSaveSlot = SaveManager.obj.GetActiveSaveProfile();
        SaveData saveData = SaveManager.obj.LoadGame(activeSaveSlot);
        if(saveData == null) {
            Debug.LogWarning("No save data found. Starting new game.");
            StartCoroutine(StartGameCoroutine());
            yield break;
        }
        
        if(saveData.background != null && saveData.background != "") {
            yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(saveData.background));
        } else {
            Debug.LogWarning("No background found in save data.");
        }

        if(saveData.surface != null && saveData.surface != "") {
            yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(saveData.surface));
        } else {
            Debug.LogWarning("No surface found in save data.");
        }

        LevelManager.obj.LoadSceneDelayed(saveData.levelId);

        GameManager.obj.IsPauseAllowed = true;

        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    public void OnStartGameButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        ShowMainMenuSubMenu(_selectSaveFileScreen, _startGameButton);
    }

    public void OnOptionsButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        ShowMainMenuSubMenu(_optionsScreen, _optionsButton);
    }

    //Special case since main menu has two canvas groups to hide/show
    public void ShowMainMenuSubMenu(UIScreen newScreen, GameObject triggerButton = null) {
        if(_isTransitioning) {
            return;
        }
        _isTransitioning = true;

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();

        if(triggerButton != null)
            _mainMenuBackSelectable = triggerButton;

        // Disable interaction on main menu
        _mainMenuScreen.interactable = false;
        _mainMenuScreen.blocksRaycasts = false;

        _mainMenuVisualsScreen.interactable = false;
        _mainMenuVisualsScreen.blocksRaycasts = false;

        // Fade out both in parallel
        _menuTransitionSequence.Append(_mainMenuScreen.DOFade(0f, UIScreen.FADE_DURATION));
        _menuTransitionSequence.Join(_mainMenuVisualsScreen.DOFade(0f, UIScreen.FADE_DURATION));

        // Deactivate after fade
        _menuTransitionSequence.AppendCallback(() =>
        {
            _mainMenuScreen.gameObject.SetActive(false);
            _mainMenuVisualsScreen.gameObject.SetActive(false);
        });

        // 🔥 Wait for Options Show tween
        _menuTransitionSequence.Append(newScreen.Show());
        _menuTransitionSequence.OnComplete(() => {
            screenStack.Push(newScreen);

            _isTransitioning = false;
        });
        
    }

    //Used to switch between sub menus. Main screen is an exception since it has two canvases, visuals and menu.
    //These two screens has to be separated since the particles screen is layered in between them.
    public void OpenScreen(UIScreen newScreen, GameObject triggerButton = null)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;

        UIScreen current = screenStack.Count > 0 ? screenStack.Peek() : null;

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();

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

        //Special case with title menu
        if(screenStack.Count == 1) {
            ShowTitleMenu();
            return;
        }

        _isTransitioning = true;

        UIScreen current = screenStack.Pop();
        UIScreen previous = screenStack.Peek();

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();

        _menuTransitionSequence.Append(current.Hide());

        if(screenStack.Count == 0) {
            ShowTitleMenu();
        } else {
            _menuTransitionSequence.Append(previous.Show());

            _menuTransitionSequence.OnComplete(() =>
            {
                _isTransitioning = false;
            });
        }
    }

    private void ShowTitleMenu() {
        if(_isTransitioning) {
            return;
        }
        _isTransitioning = true;

        UIScreen current = screenStack.Pop();

        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = DOTween.Sequence();

        _menuTransitionSequence.Append(current.Hide());

        _menuTransitionSequence.AppendCallback(() =>
        {
            _mainMenuScreen.gameObject.SetActive(true);
            _mainMenuVisualsScreen.gameObject.SetActive(true);
            if(_mainMenuBackSelectable != null)
                EventSystem.current.SetSelectedGameObject(_mainMenuBackSelectable);

            _mainMenuScreen.alpha = 0f;
            _mainMenuVisualsScreen.alpha = 0f;

            _mainMenuScreen.interactable = false;
            _mainMenuScreen.blocksRaycasts = false;

            _mainMenuVisualsScreen.interactable = false;
            _mainMenuVisualsScreen.blocksRaycasts = false;
        });

        _menuTransitionSequence.Append(_mainMenuScreen.DOFade(1f, UIScreen.FADE_DURATION));
        _menuTransitionSequence.Join(_mainMenuVisualsScreen.DOFade(1f, UIScreen.FADE_DURATION));

        _menuTransitionSequence.OnComplete(() =>
        {
            _mainMenuScreen.interactable = true;
            _mainMenuScreen.blocksRaycasts = true;

            _mainMenuVisualsScreen.interactable = true;
            _mainMenuVisualsScreen.blocksRaycasts = true;

            _isTransitioning = false;
        });
    }

    public void OnGameOptionsButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        OpenScreen(_gameOptionsScreen, _optionsMenuGameOptionsButton);
    }

    public void OnAudioMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        OpenScreen(_audioScreen, _optionsMenuAudioButton);
    }

    public void OnKeyboardMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        OpenScreen(_keyboardScreen, _optionsMenuKeyboardButton);
    }

    public void OnControllerMenuButtonClicked() {
        UISoundPlayer.obj.PlaySelect();
        OpenScreen(_controllerScreen, _optionsMenuControllerButton);
    }

    public void ExitGame()
    {
        UISoundPlayer.obj.PlaySelect();
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    private void OnCancel(InputAction.CallbackContext context) {
        if(screenStack.Count >= 1)
            UISoundPlayer.obj.PlayBack();
        GoBack();
    }
}
