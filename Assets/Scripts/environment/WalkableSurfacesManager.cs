using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WalkableSurfacesManager : MonoBehaviour
{
    public static WalkableSurfacesManager obj;
    private HashSet<string> _loadedSurfaces;

    void Awake() {
        obj = this;
        _loadedSurfaces = new();

        //Check any children and add them to loadedSurfaces
        WalkableSurface[] walkableSurfaces = GetComponentsInChildren<WalkableSurface>();
        foreach(WalkableSurface walkableSurface in walkableSurfaces) {
            _loadedSurfaces.Add(walkableSurface.walkableSurfaceScene.SceneName);
        }
    }

    public bool IsSurfaceLoaded(SceneField walkableSurfaceScene) {
        return IsSurfaceLoaded(walkableSurfaceScene.SceneName);
    }

    public bool IsSurfaceLoaded(string walkableSurfaceName) {
        MaybeRemoveUnusedSurfaces(walkableSurfaceName);

        if(_loadedSurfaces.Contains(walkableSurfaceName)) {
            Debug.Log("Surface already loaded: " + walkableSurfaceName);
            return true;
        }
        
        Debug.Log("Surface not loaded: " + walkableSurfaceName);
        return false;
    }

    private string _lastCheckedSceneName;
    private int _consequetiveChecks = 0;
    private readonly int _maxConsequetiveChecks = 5;
    private void MaybeRemoveUnusedSurfaces(string sceneName) {
        _lastCheckedSceneName ??= sceneName;

        if(sceneName == _lastCheckedSceneName) {
            _consequetiveChecks++;
            Debug.Log("Consequetive checks: " + _consequetiveChecks);
            if(_consequetiveChecks > _maxConsequetiveChecks) {
                WalkableSurface[] walkableSurfaces = GetComponentsInChildren<WalkableSurface>();
                if(walkableSurfaces.Length > 1) {
                    foreach(WalkableSurface walkableSurface in walkableSurfaces) {
                        if(walkableSurface.walkableSurfaceScene.SceneName != sceneName) {
                            RemoveWalkableSurface(walkableSurface.walkableSurfaceScene);
                        }
                    }
                }
            }
        } else {
            _consequetiveChecks = 0;
            _lastCheckedSceneName = sceneName;
        }
    }

    public IEnumerator AddWalkableSurface(SceneField walkableSurfaceSceneField) {
        yield return StartCoroutine(AddWalkableSurface(walkableSurfaceSceneField.SceneName));
    }

    public IEnumerator AddWalkableSurface(string walkableSurfaceName) {
        if(IsSurfaceLoaded(walkableSurfaceName))
            yield break;

        _loadedSurfaces.Add(walkableSurfaceName);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(walkableSurfaceName, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }
        GameObject walkableSurface = SceneManager.GetSceneByName(walkableSurfaceName).GetRootGameObjects().First(gameObject => gameObject.CompareTag("WalkableSurface"));
        walkableSurface.transform.parent = transform;
        SceneManager.UnloadSceneAsync(walkableSurfaceName);

        yield return null;
    }

    private void RemoveWalkableSurface(SceneField walkableSurfaceScene) {
        if(!_loadedSurfaces.Contains(walkableSurfaceScene.SceneName))
            return;


        WalkableSurface[] walkableSurfaces = GetComponentsInChildren<WalkableSurface>();
        if(walkableSurfaces == null || walkableSurfaces.Length == 0)
            return;

        foreach(WalkableSurface walkableSurface in walkableSurfaces) {
            if(walkableSurfaceScene.SceneName == walkableSurface.walkableSurfaceScene.SceneName) {
                Destroy(walkableSurface.gameObject);
                _loadedSurfaces.Remove(walkableSurfaceScene.SceneName);
                Debug.Log("Removed surface: " + walkableSurfaceScene.SceneName);
                return;
            }
        }
    }

    public void RemoveAllSurfaces() {
        if(_loadedSurfaces.Count == 0)
            return;

        WalkableSurface[] walkableSurfaces = GetComponentsInChildren<WalkableSurface>();
        foreach(WalkableSurface walkableSurface in walkableSurfaces) {
            Destroy(walkableSurface.gameObject);
        }
        _loadedSurfaces.Clear();
    }

    void OnDestroy()
    {
        obj = null;
    }
}
