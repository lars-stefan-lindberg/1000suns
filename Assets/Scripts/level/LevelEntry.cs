using System.Collections;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelEntry : MonoBehaviour
{
    [SerializeField] private bool _activateAlternativeCamera = false;
    [SerializeField] private bool _activateFollowCamera = false;
    [SerializeField] private bool _fireCustomCameraHandlingEvent = false;
    [SerializeField] private bool _enablePlayerTransition = true;
    public UnityEvent OnCustomCameraHandling;
    private BoxCollider2D _collider;
    private SpawnPoint _spawnPoint;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
        _spawnPoint = GetComponentInChildren<SpawnPoint>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Scene exitedScene = SceneManager.GetActiveScene();
            Scene newScene = gameObject.scene;

            if(exitedScene == newScene)
                return;

            if(_enablePlayerTransition)
                PlayerManager.obj.SetTransitioningBetweenLevels();

            LevelManager.obj.LoadAdjacentRooms(newScene);
            LevelManager.obj.UnloadNonAdjacentRooms(newScene);

            LevelTracker.obj.StartTimeTracking(newScene.name);
            LevelTracker.obj.StopTimeTracking(exitedScene.name);
            SceneManager.SetActiveScene(newScene);

            PlayerManager.PlayerType playerType = PlayerManager.obj.GetPlayerTypeFromCollider(other);
            PlayerManager.PlayerDirection playerDirection = GetPlayerDirection(other);
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            CinemachineVirtualCamera activeVirtualCamera = (CinemachineVirtualCamera) brain.ActiveVirtualCamera;
            StartCoroutine(ActivateNextRoomCameraAndTransitionPlayer(newScene, activeVirtualCamera, playerDirection, playerType));

            GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);

            SaveManager.obj.SaveGame(newScene.name);
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

    public IEnumerator ActivateNextRoomCameraAndTransitionPlayer(Scene sceneToActivate, CinemachineVirtualCamera activeCamera, PlayerManager.PlayerDirection direction, PlayerManager.PlayerType playerType) {
        if(_enablePlayerTransition) {
            PlayerManager.obj.TransitionToNextRoom(direction, playerType);
        }

        activeCamera.gameObject.SetActive(false);
        activeCamera.enabled = false;

        if(_fireCustomCameraHandlingEvent) {
            OnCustomCameraHandling?.Invoke();
        } else {
            GameObject[] sceneGameObjects = sceneToActivate.GetRootGameObjects();
            GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
            CameraManager cameraManager = cameras.GetComponent<CameraManager>();
            if(_activateAlternativeCamera) {
                cameraManager.ActivateAlternativeCamera();
            } else if(_activateFollowCamera) {
                var playerTranform = PlayerManager.obj.GetPlayerTransform(playerType);
                cameraManager.ActivateFollowCamera(playerTranform);
            } else {
                cameraManager.ActivateMainCamera();
            }
        }

        yield return null;
    }
}
