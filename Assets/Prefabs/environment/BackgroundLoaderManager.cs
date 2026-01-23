using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundLoaderManager : MonoBehaviour
{
    public static BackgroundLoaderManager obj;

    void Awake()
    {
        obj = this;
    }

    public bool IsBackgroundLayersLoaded(SceneField backgroundScene) {
        Camera mainCamera = Camera.main;
        if(mainCamera != null) {
            BackgroundLayersManager backgroundLayersManager = mainCamera.gameObject.GetComponentInChildren<BackgroundLayersManager>();
            if(backgroundLayersManager != null) {
                return backgroundLayersManager.backgroundScene.SceneName == backgroundScene.SceneName;
            }
        }
        return false;
    }

    public IEnumerator LoadAndSetBackground(SceneField backgroundScene) {
        yield return StartCoroutine(LoadAndSetBackground(backgroundScene.SceneName));
    }

    public IEnumerator LoadAndSetBackground(string backgroundScene) {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(backgroundScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }
        GameObject backgroundGameObject = SceneManager.GetSceneByName(backgroundScene).GetRootGameObjects().First(gameObject => gameObject.CompareTag("BackgroundLayers"));
        backgroundGameObject.transform.parent = Camera.main.gameObject.transform;
        backgroundGameObject.GetComponent<BackgroundLayersManager>().isActive = true;

        SceneManager.UnloadSceneAsync(backgroundScene);
        
        yield return null;
    }

    public void RemoveBackgroundLayers() {
        Camera mainCamera = Camera.main;
        if(mainCamera != null) {
            BackgroundLayersManager backgroundLayersManager = mainCamera.gameObject.GetComponentInChildren<BackgroundLayersManager>();
            if(backgroundLayersManager != null) {
                Destroy(backgroundLayersManager.gameObject);
            }
        }
    }

    void OnDestroy()
    {
        obj = null;
    }
}
