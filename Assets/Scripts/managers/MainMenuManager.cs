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
    [SerializeField] private CanvasGroup _mainMenu;
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
    [SerializeField] private SceneField _firstForestBackground;
    [SerializeField] private SceneField _firstForestSurfaces;
    [SerializeField] private GameObject _titleScreenCanvas;
    [SerializeField] private GameObject _titleScreenBackgroundCanvas;
    [SerializeField] private GameObject _particlesCanvas;
    [SerializeField] private GameObject _titleTextCanvas;
    [SerializeField] private GameObject _lightsCanvas;
    [SerializeField] private GameObject _playButton;
    [SerializeField] private GameObject _glitchKeyStudios;
    [SerializeField] private GameObject _fmod;
    [SerializeField] private GameObject _saveInfo;
    [SerializeField] private InputActionReference _cancelActionReference;
    [SerializeField] private InputActionAsset actions;
 
    private Sequence _menuTransitionSequence;
    private bool _isTransitioning = false;
    private Stack<UIScreen> screenStack = new();
    private GameObject _mainMenuBackSelectable;
    private bool _ignoreCancelInput = false;

    void Awake() {
        obj = this;
    }

    void Start() {
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

        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    private void SetCanvasCamera(Canvas canvas) {
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
    }

    public void OnBoot() {
        StartCoroutine(OnBootSequence());
    }

    public void OnReturn() {
        StartCoroutine(OnReturnSequence());
    }

    private IEnumerator OnBootSequence() {
        _titleScreenCanvas.SetActive(true);
        _titleScreenBackgroundCanvas.SetActive(false);
        _glitchKeyStudios.SetActive(true);

        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;
        yield return new WaitForSeconds(2f);

        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;
        _glitchKeyStudios.SetActive(false);

        _fmod.SetActive(true);
        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;
        yield return new WaitForSeconds(2f);
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;
        _fmod.SetActive(false);

        _saveInfo.SetActive(true);
        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;
        yield return new WaitForSeconds(3f);
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;
        _saveInfo.SetActive(false);

        _titleScreenBackgroundCanvas.SetActive(true);
        _particlesCanvas.SetActive(true);
        _lightsCanvas.SetActive(true);
        _titleTextCanvas.SetActive(true);
        _mainMenu.alpha = 0;
        _mainMenu.interactable = false;
        _mainMenu.blocksRaycasts = false;
        
        yield return new WaitForSeconds(1f);

        SceneFadeManager.obj.StartFadeIn(0.5f);
        MusicManager.obj.Play(_titleScreenMusic);

        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        yield return new WaitForSeconds(0.3f);
        
        EventSystem.current.SetSelectedGameObject(_playButton);
        _mainMenu
            .DOFade(1f, 0.4f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _mainMenu.interactable = true;
                _mainMenu.blocksRaycasts = true;
            });

        _cancelActionReference.action.performed += OnCancel;
    }

    public IEnumerator OnReturnSequence() {
        _titleScreenCanvas.SetActive(true);
        _particlesCanvas.SetActive(true);
        _lightsCanvas.SetActive(true);
        _titleTextCanvas.SetActive(true);
        _mainMenu.alpha = 0;
        _mainMenu.interactable = false;
        _mainMenu.blocksRaycasts = false;

        yield return new WaitForSeconds(1f);

        SceneFadeManager.obj.StartFadeIn(1f);
        MusicManager.obj.Play(_titleScreenMusic);

        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        EventSystem.current.SetSelectedGameObject(_playButton);
        _mainMenu
            .DOFade(1f, 0.3f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _mainMenu.interactable = true;
                _mainMenu.blocksRaycasts = true;
            });

        _cancelActionReference.action.performed += OnCancel;

        yield return null;
    }

    void OnDestroy() {
        _cancelActionReference.action.performed -= OnCancel;
        
        // Kill the main menu transition sequence
        _menuTransitionSequence?.Kill();
        _menuTransitionSequence = null;
        
        // Kill all tweens on the main menu canvas groups
        if (_mainMenu != null) DOTween.Kill(_mainMenu);
        if (_mainMenuScreen != null) DOTween.Kill(_mainMenuScreen);
        if (_mainMenuVisualsScreen != null) DOTween.Kill(_mainMenuVisualsScreen);
        
        // Kill all tweens on sub-screens and their children
        if (_selectSaveFileScreen != null) {
            DOTween.Kill(_selectSaveFileScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_selectSaveFileScreen.transform, true); // true = complete all child tweens too
        }
        if (_optionsScreen != null) {
            DOTween.Kill(_optionsScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_optionsScreen.transform, true);
        }
        if (_gameOptionsScreen != null) {
            DOTween.Kill(_gameOptionsScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_gameOptionsScreen.transform, true);
        }
        if (_audioScreen != null) {
            DOTween.Kill(_audioScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_audioScreen.transform, true);
        }
        if (_controllerScreen != null) {
            DOTween.Kill(_controllerScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_controllerScreen.transform, true);
        }
        if (_keyboardScreen != null) {
            DOTween.Kill(_keyboardScreen.GetComponent<CanvasGroup>());
            DOTween.Kill(_keyboardScreen.transform, true);
        }
        
        // Kill all tweens on main menu canvases and their children
        if (_titleScreenCanvas != null) DOTween.Kill(_titleScreenCanvas.transform, true);
        if (_particlesCanvas != null) DOTween.Kill(_particlesCanvas.transform, true);
        if (_titleTextCanvas != null) DOTween.Kill(_titleTextCanvas.transform, true);
        if (_lightsCanvas != null) DOTween.Kill(_lightsCanvas.transform, true);
        
        obj = null;
    }

    public void OnNewGameButtonClicked() {
        StartGame();
    }

    public void StartGame() {
        _ignoreCancelInput = true;
        StartCoroutine(StartGameCoroutine());
    }

    public void ContinueGame() {
        _ignoreCancelInput = true;
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

        AsyncOperation loadIntroSceneOperation = SceneManager.LoadSceneAsync(_introScene, LoadSceneMode.Additive);
        while(!loadIntroSceneOperation.isDone) {
            yield return null;
        }
        Scene introScene = SceneManager.GetSceneByName(_introScene.SceneName);
        SceneManager.SetActiveScene(introScene);
        
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
        GameManager.obj.ResumeTimerOnContinueGame = true;

        Player.obj.gameObject.SetActive(false);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        PlayerManager.obj.IsSeparated = false;
        PlayerManager.obj.IsCoopActive = false;

        int activeSaveSlot = SaveManager.obj.GetActiveSaveProfile();
        var loadTask = SaveManager.obj.LoadGame(activeSaveSlot);
        while (!loadTask.IsCompleted) {
            yield return null;
        }
        SaveData saveData = loadTask.Result;
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
        if (_ignoreCancelInput)
            return;
            
        if(screenStack.Count >= 1)
            UISoundPlayer.obj.PlayBack();
        GoBack();
    }

    public void IgnoreCancelInputTemporarily(float duration = 0.3f) {
        if (_ignoreCancelInputCoroutine != null)
            StopCoroutine(_ignoreCancelInputCoroutine);
        _ignoreCancelInputCoroutine = StartCoroutine(IgnoreCancelInputCoroutine(duration));
    }

    private Coroutine _ignoreCancelInputCoroutine;
    private IEnumerator IgnoreCancelInputCoroutine(float duration) {
        _ignoreCancelInput = true;
        yield return new WaitForSecondsRealtime(duration);
        _ignoreCancelInput = false;
        _ignoreCancelInputCoroutine = null;
    }
}
