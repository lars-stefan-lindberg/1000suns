using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempBgLoaderTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        //Get bg scene id from active scene
        SceneMetadata metadata = LevelManager.obj.GetActiveSceneMetadata();
        SceneField backgroundScene = metadata.backgroundScene;
        //Check if bg is already loaded and on main camera
        if (backgroundScene != null && !LevelManager.obj.IsBackgroundLayersLoaded(backgroundScene))
        {
            //Load background scene and add the background to main camera
            StartCoroutine(LoadAndSetBackground(backgroundScene));
        }
    }

    private IEnumerator LoadAndSetBackground(SceneField backgroundScene) {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(backgroundScene.SceneName, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }
        GameObject backgroundGameObject = SceneManager.GetSceneByName(backgroundScene.SceneName).GetRootGameObjects().First(gameObject => gameObject.CompareTag("BackgroundLayers"));
        backgroundGameObject.transform.parent = Camera.main.gameObject.transform;
        backgroundGameObject.GetComponent<BackgroundLayersManager>().isActive = true;

        SceneManager.UnloadSceneAsync(backgroundScene.SceneName);
    }
}
