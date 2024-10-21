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
        SceneManager.LoadSceneAsync(_commonObjects, LoadSceneMode.Additive);
        Scene commonObjects = SceneManager.GetSceneByName(_commonObjects.SceneName);
        while(!commonObjects.isLoaded) {
            yield return null;
        }

        SceneManager.LoadSceneAsync(_titleScreen, LoadSceneMode.Additive);
        Scene titleScreen = SceneManager.GetSceneByName(_titleScreen.SceneName);
        while(!titleScreen.isLoaded) {
            yield return null;
        }

        SceneManager.UnloadSceneAsync(_bootGame.SceneName);
    }
}
