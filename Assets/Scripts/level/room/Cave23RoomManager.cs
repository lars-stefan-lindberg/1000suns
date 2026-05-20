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
    [SerializeField] private MusicTrack _caveMain;

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
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.IsFollowingPlayer = false;
        AmbienceManager.obj.Stop();
        SoundFXManager.obj.Play2D(_teleportSfx);
        StartCoroutine(TeleportToDreamRoomRoutine());
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        SceneFadeManager.obj.StartWhiteFadeOut(0.5f);

        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

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
        MusicManager.obj.Play(_caveMain);
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
