using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour
{
    [SerializeField] private SceneField _currentScene;
    [SerializeField] private SceneField _nextScene;
    [SerializeField] private GameObject _currentRoomCamera;
    [SerializeField] private SceneField[] _scenesToLoad;
    [SerializeField] private SceneField[] _scenesToUnload;
    [SerializeField] private bool _enablePlayerTransition = true;
    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(SceneManager.GetActiveScene().name != _currentScene.SceneName) {
            return;
        }
        if(other.CompareTag("Player")) {
            if(_enablePlayerTransition)
                PlayerManager.obj.SetTransitioningBetweenLevels();

            StartCoroutine(LoadScenesCoroutine());
            StartCoroutine(UnloadScenes());
            
            LevelTracker.obj.StartTimeTracking(_nextScene.SceneName);
            LevelTracker.obj.StopTimeTracking(_currentScene.SceneName);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_nextScene));

            PlayerManager.PlayerDirection playerDirection = GetPlayerDirection(other);
            StartCoroutine(ActivateNextRoomCameraAndTransitionPlayer(playerDirection));

            StartCoroutine(MutePrisonersOffscreen());
            StartCoroutine(UnmutePrisonersOnScreen());
        }
    }

    private float _collisionMargin = 0.5f;
    private PlayerManager.PlayerDirection GetPlayerDirection(Collider2D playerCollider) {
        Bounds playerCollisionBounds = playerCollider.bounds;
        Bounds levelSwitcherBounds = _collider.bounds;

        //Check if player collides falling down -> direction down
        Vector2 playerBottom = new(playerCollisionBounds.center.x, playerCollisionBounds.center.y - playerCollisionBounds.extents.y);
        Vector2 levelSwitcherTop = new(levelSwitcherBounds.center.x, levelSwitcherBounds.center.y + levelSwitcherBounds.extents.y); 
        if(playerBottom.y > levelSwitcherTop.y - _collisionMargin)
            return PlayerManager.PlayerDirection.DOWN;

        //Check if player colliders jumping up -> direction up
        Vector2 playerTop = new(playerCollisionBounds.center.x, playerCollisionBounds.center.y + playerCollisionBounds.extents.y);
        Vector2 levelSwitcherBottom = new(levelSwitcherBounds.center.x, levelSwitcherBounds.center.y - levelSwitcherBounds.extents.y); 
        if(playerTop.y < levelSwitcherBottom.y + _collisionMargin)
            return PlayerManager.PlayerDirection.UP;

        //Check if player collides from right -> direction left
        if(playerCollisionBounds.center.x > levelSwitcherBounds.center.x)
            return PlayerManager.PlayerDirection.LEFT;
        
        //Otherwise direction right
        return PlayerManager.PlayerDirection.RIGHT;
    }

    public IEnumerator ActivateNextRoomCameraAndTransitionPlayer(PlayerManager.PlayerDirection direction) {
        if(_enablePlayerTransition) {
            if(_scenesToLoad.Length > 0) {
                bool scenesLoaded = false;
                while(!scenesLoaded) {
                    for(int i = 0; i < _scenesToLoad.Length; i++) {
                        Scene scene = SceneManager.GetSceneByName(_scenesToLoad[i].SceneName);
                        if(!scene.isLoaded) {
                            scenesLoaded = false;
                        } else if(i == _scenesToLoad.Length - 1 && scene.isLoaded)
                            scenesLoaded = true;
                    }
                    yield return null;
                }
            }

            PlayerManager.obj.TransitionToNextRoom(direction);
        }

        _currentRoomCamera.SetActive(false);
        _currentRoomCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;

        GameObject[] sceneGameObjects = SceneManager.GetSceneByName(_nextScene).GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        cameraManager.ActivateMainCamera(direction);

        yield return null;
    }

    public void LoadNextRoom() {
        StartCoroutine(LoadNextRoomCoroutine());
    }

    private IEnumerator LoadNextRoomCoroutine() {
        Scene nextScene = SceneManager.GetSceneByName(_nextScene.SceneName);
        if(!nextScene.isLoaded)
            SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        yield return null;
    }

    private IEnumerator LoadScenesCoroutine() {
        for(int i = 0; i < _scenesToLoad.Length; i++) 
        {
            bool isSceneLoaded = false;
            for(int j = 0; j < SceneManager.sceneCount; j++) {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if(loadedScene.name == _scenesToLoad[i].SceneName) {
                    isSceneLoaded = true;
                    break;
                }
            }

            if(!isSceneLoaded) {
                SceneManager.LoadSceneAsync(_scenesToLoad[i], LoadSceneMode.Additive);
            }
        }
        yield return null;
    }

    private IEnumerator UnloadScenes() {
        for(int i = 0; i < _scenesToUnload.Length; i++) {
            for(int j = 0; j < SceneManager.sceneCount; j++) {
                Scene loadedScene = SceneManager.GetSceneAt(j);
                if(loadedScene.name == _scenesToUnload[i].SceneName) {
                    SceneManager.UnloadSceneAsync(_scenesToUnload[i]);
                }
            }
        }
        yield return null;
    }

    private IEnumerator MutePrisonersOffscreen() {
        GameObject[] sceneGameObjects = SceneManager.GetSceneByName(_currentScene).GetRootGameObjects();
        IEnumerable<GameObject> prisonerGameObjects = sceneGameObjects.Where(gameObject => gameObject.CompareTag("Enemy"));
        foreach(GameObject gameObject in prisonerGameObjects) {
            Prisoner prisoner = gameObject.GetComponent<Prisoner>();
            prisoner.offScreen = true;
        }
        yield return null;
    }

    private IEnumerator UnmutePrisonersOnScreen() {
        GameObject[] sceneGameObjects = SceneManager.GetSceneByName(_nextScene).GetRootGameObjects();
        IEnumerable<GameObject> prisonerGameObjects = sceneGameObjects.Where(gameObject => gameObject.CompareTag("Enemy"));
        foreach(GameObject gameObject in prisonerGameObjects) {
            Prisoner prisoner = gameObject.GetComponent<Prisoner>();
            prisoner.offScreen = false;
        }
        yield return null;
    }
}
