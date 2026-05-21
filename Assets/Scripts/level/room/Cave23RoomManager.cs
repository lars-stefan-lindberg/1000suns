using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave23RoomManager : MonoBehaviour
{
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _eliReturnFromDreamRoomPosition;
    [SerializeField] private Transform _sootStartPositionAfterDreamRoom;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private GameObject _crystalCutsceneCamera;

    void Start() {
        //If coming back from dream room, load room state
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
            _conversationManager.OnConversationEnd += OnConversationCompleted;
            StartCoroutine(AfterEliDreamRoom());
        }
    }

    private void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    private IEnumerator AfterEliDreamRoom() {
        AmbienceManager.obj.Stop();
        AmbienceManager.obj.Play(_caveMainAmbience);
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;
        PlayerMovement.obj.SetNewPower();

        //Set Soot start position
        CaveAvatar.obj.SetPosition(_sootStartPositionAfterDreamRoom.position, false);
        CaveAvatar.obj.SetFlipX(true);

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;


        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);

        StartCoroutine(SetupDialogue());
    }
    
    public void TeleportToDreamRoom() {
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted))
            return;
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.IsFollowingPlayer = false;
        AmbienceManager.obj.Stop();
        
        StartCoroutine(TeleportToDreamRoomRoutine());
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        yield return new WaitForSeconds(1f);
        _crystalCutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        PlayerMovement.obj.StartWalking();
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));

        yield return new WaitForSeconds(2.25f);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.StopWalking();
        yield return null;
        PlayerMovement.obj.SetNewPower();
        yield return new WaitForSeconds(0.05f);
        CameraShakeManager.obj.ForcePushShake();
        yield return new WaitForSeconds(1.5f);

        PlayerMovement.obj.IsControlledProgrammatically = true;
        Player.obj.rigidBody.gravityScale = 0;

        //Move player up in the air
        float startY = Player.obj.transform.position.y;
        float targetY = startY + 3f;
        float maxSpeed = 2f;
        float acceleration = 1f;
        float deceleration = 1f;
        float currentSpeed = 0f;

        CameraShakeManager.obj.ForcePushShake();
        while (Player.obj.transform.position.y < targetY) {
            float distanceRemaining = targetY - Player.obj.transform.position.y;
            float stoppingDistance = (currentSpeed * currentSpeed) / (2f * deceleration);
            
            if (distanceRemaining <= stoppingDistance) {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            } else {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            
            Vector3 pos = Player.obj.transform.position;
            pos.y += currentSpeed * Time.deltaTime;
            pos.y = Mathf.Min(pos.y, targetY);
            Player.obj.transform.position = pos;
            yield return null;
        }
        
        SoundFXManager.obj.Play2D(_teleportSfx);
        SceneFadeManager.obj.StartWhiteFadeOut(0.5f);

        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        Player.obj.gameObject.SetActive(false);
        Player.obj.rigidBody.gravityScale = 1;
        PlayerMovement.obj.IsControlledProgrammatically = false;
        _crystalCutsceneCamera.SetActive(false);

        //Load dream room
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_dreamRoomScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }

        //Give some time for dream room to load until unloading current room
        yield return new WaitForSeconds(2f);

        //Unload current room
        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
    }

    private IEnumerator SetupDialogue() {
        yield return new WaitForSeconds(0.5f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.IsFollowingPlayer = true;
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
