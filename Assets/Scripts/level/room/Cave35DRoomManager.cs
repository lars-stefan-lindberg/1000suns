using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.InputSystem;

public class Cave35DRoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _eliSpawnPoint;
    [SerializeField] private AmbienceTrack _capeRoomAmbience;
    [SerializeField] private TutorialStrip _tutorialStrip;
    [SerializeField] private LocalizedString _basicMovementString;
    [SerializeField] private List<InputActionReference> _basicMovementActions;
    [SerializeField] private List<InputIconManager.Direction> _basicMovementDirections;

    [SerializeField] private LocalizedString _fullyChargedString;
    [SerializeField] private List<InputActionReference> _fullyChargedActions;
    [SerializeField] private List<InputIconManager.Direction> _fullyChargedDirections;

    void Start() {
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.FlipPlayer();
        Player.obj.transform.position = _eliSpawnPoint.transform.position;
        PlayerMovement.obj.SetNewPower();
        DustParticleMgr.obj.Enabled = false;
        AmbienceManager.obj.Play(_capeRoomAmbience);
        StartCoroutine(TransitionIntoRoom());

        var steps = new List<ConditionalTutorialStep>
        {
            // Step 0: Basic movement (default, shown when no other condition is true)
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _basicMovementString,
                    inputActions = _basicMovementActions,
                    inputDirections = _basicMovementDirections
                },
                condition = null // No condition = default step
            },
            
            // Step 1: Wall latch tutorial (shown when latched to wall)
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _fullyChargedString,
                    inputActions = _fullyChargedActions,
                    inputDirections = _fullyChargedDirections
                },
                condition = () => PlayerPush.obj != null && PlayerPush.obj.IsFullyCharged()
            }
        };
        
        _tutorialStrip.InitializeConditional(steps);
    }

    private IEnumerator TransitionIntoRoom() {
        //Set camera
        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliSpawnPoint.transform.position);
        yield return new WaitForSeconds(1f);
        SceneManager.SetActiveScene(gameObject.scene);

        //All loading should be completed. Start fading in room
        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
    }

    public void ShowTutorialStrip() {

        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        _tutorialStrip.Hide();
    }
}
