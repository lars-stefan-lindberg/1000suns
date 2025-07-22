using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootGame : MonoBehaviour
{
    [SerializeField] private SceneField _commonObjects;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private SceneField _bootGame;

    void Start()
    {
        StartCoroutine(Boot());
    }

    private IEnumerator Boot() {
        AsyncOperation loadCommonOperation = SceneManager.LoadSceneAsync(_commonObjects, LoadSceneMode.Additive);
        while(!loadCommonOperation.isDone) {
            yield return null;
        }
        GeneralTracker.obj.TrackGameStarted();

        AsyncOperation loadTitleScreenOperation = SceneManager.LoadSceneAsync(_titleScreen, LoadSceneMode.Additive);
        while(!loadTitleScreenOperation.isDone) {
            yield return null;
        }
        SceneFadeManager.obj.SetFadedInState();

        SceneManager.UnloadSceneAsync(_bootGame.SceneName);
    }
}
