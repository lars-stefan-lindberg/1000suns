using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;
using TMPro;

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
    private Color _optionsButtonColor;
    [SerializeField] private Button _backButton;
    private Color _backButtonColor;
    [SerializeField] private GameObject _musicSlider;

    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _titleMenu;

    void Awake() {
        EventSystem.current.SetSelectedGameObject(_playButton);
        _optionsButtonColor = _optionsButton.GetComponentInChildren<TextMeshProUGUI>().color;
        _backButtonColor = _backButton.GetComponentInChildren<TextMeshProUGUI>().color;
        MusicManager.obj.PlayTitleSong();
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
        float masterVolume = SoundMixerManager.obj.GetMasterVolume();

        StartCoroutine(SoundMixerManager.obj.StartMasterFade(3f, 0.001f));
        StartFadeOut();
        while(IsFadingOut)
            yield return null;
        while(SoundMixerManager.obj.GetMasterVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();

        SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        Scene persistentGameplay = SceneManager.GetSceneByName(_persistentGameplay.SceneName);
        while(!persistentGameplay.isLoaded) {
            yield return null;
        }
        Player.obj.SetHasCape(false);

        SoundMixerManager.obj.SetMasterVolume(masterVolume);
        SceneManager.LoadSceneAsync(_caveRoom1, LoadSceneMode.Additive);
        Scene caveRoom1 = SceneManager.GetSceneByName(_caveRoom1.SceneName);
        while(!caveRoom1.isLoaded) {
            yield return null;
        }
        Player.obj.SetCaveStartingCoordinates();
        
        GameObject[] sceneGameObjects = caveRoom1.GetRootGameObjects();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        RoomMgr roomMgr = room.GetComponentInChildren<RoomMgr>();
        roomMgr.ActivateVirtualCamera();

        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    private void StartFadeOut() {
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }

    public void ShowOptionsMenu() {
        _titleMenu.SetActive(false);
        _optionsMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_musicSlider);

        //Reset color of options button from animation
        TextMeshProUGUI textMeshPro = _optionsButton.GetComponentInChildren<TextMeshProUGUI>();
        textMeshPro.color = _optionsButtonColor;
    }

    public void ShowTitleMenu() {
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
}
