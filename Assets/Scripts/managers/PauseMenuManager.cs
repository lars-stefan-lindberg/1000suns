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
    
    private Color _buttonColor;

    private bool _isPaused = false;

    void Awake() {
        obj = this;
        _buttonColor = _menuObjects[0].GetComponentInChildren<TextMeshProUGUI>().color;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_isPaused) {
                ResumeGame();
            } else {
                PlayerMovement.obj.DisablePlayerMovement();
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
        PlayerMovement.obj.EnablePlayerMovement();
        foreach(GameObject gameObject in _menuObjects) {
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = _buttonColor;
        }
        Time.timeScale = 1f;
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
