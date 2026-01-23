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
        MaybeRemoveUnusedSurfaces(walkableSurfaceScene);

        if(_loadedSurfaces.Contains(walkableSurfaceScene.SceneName)) {
            Debug.Log("Surface already loaded: " + walkableSurfaceScene.SceneName);
            return true;
        }
        
        Debug.Log("Surface not loaded: " + walkableSurfaceScene.SceneName);
        return false;
    }

    private SceneField _lastCheckedSceneField;
    private int _consequetiveChecks = 0;
    private readonly int _maxConsequetiveChecks = 5;
    private void MaybeRemoveUnusedSurfaces(SceneField sceneField) {
        _lastCheckedSceneField ??= sceneField;

        if(sceneField.SceneName == _lastCheckedSceneField.SceneName) {
            _consequetiveChecks++;
            Debug.Log("Consequetive checks: " + _consequetiveChecks);
            if(_consequetiveChecks > _maxConsequetiveChecks) {
                WalkableSurface[] walkableSurfaces = GetComponentsInChildren<WalkableSurface>();
                if(walkableSurfaces.Length > 1) {
                    foreach(WalkableSurface walkableSurface in walkableSurfaces) {
                        if(walkableSurface.walkableSurfaceScene.SceneName != sceneField.SceneName) {
                            RemoveWalkableSurface(walkableSurface.walkableSurfaceScene);
                        }
                    }
                }
            }
        } else {
            _consequetiveChecks = 0;
            _lastCheckedSceneField = sceneField;
        }
    }

    public IEnumerator AddWalkableSurface(SceneField walkableSurfaceSceneField) {
        if(IsSurfaceLoaded(walkableSurfaceSceneField))
            yield break;

        _loadedSurfaces.Add(walkableSurfaceSceneField.SceneName);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(walkableSurfaceSceneField.SceneName, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }
        GameObject walkableSurface = SceneManager.GetSceneByName(walkableSurfaceSceneField.SceneName).GetRootGameObjects().First(gameObject => gameObject.CompareTag("WalkableSurface"));
        walkableSurface.transform.parent = transform;
        SceneManager.UnloadSceneAsync(walkableSurfaceSceneField.SceneName);

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
