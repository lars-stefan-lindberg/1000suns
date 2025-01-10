using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager obj;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _firstSelectedPauseMenuItem;
    [SerializeField] private GameObject[] _menuObjects;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private GameObject _persistentGameplay;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundFXSlider;
    [SerializeField] private Slider _ambienceSlider;
    [SerializeField] private TextMeshProUGUI _collectibleCountText;
    
    private Color _buttonColor;

    private bool _isPaused = false;

    void Awake() {
        obj = this;
        _buttonColor = _menuObjects[0].GetComponentInChildren<TextMeshProUGUI>().color;
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
            
        for (int i = 0; i < _menuObjects.Length; i++)
        {
            _menuObjects[i].GetComponentInChildren<TextMeshProUGUI>().color = _buttonColor;
        }
        Time.timeScale = 1f;
        if(DialogueController.obj != null && DialogueController.obj.IsDisplayed()) {
            DialogueController.obj.FocusDialogue();
        } else if(TutorialDialogManager.obj != null && !TutorialDialogManager.obj.tutorialCompleted) {
            TutorialDialogManager.obj.Focus();
        } else {
            PlayerMovement.obj.EnablePlayerMovement();
        }
        _isPaused = false;
    }

    public void QuitButtonHandler() {
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
        SoundMixerManager.obj.SetMusicVolume(volume);
    }

    public void ChangeSoundFxVolume(float volume) {
        SoundMixerManager.obj.SetSoundFXVolume(volume);
    }
    
    public void ChangeAmbienceVolume(float volume) {
        SoundMixerManager.obj.SetAmbienceVolume(volume);
    }

    void OnDestroy() {
        obj = null;
    }
}
