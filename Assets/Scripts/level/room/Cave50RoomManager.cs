using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave50RoomManager : MonoBehaviour
{
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _eliReturnFromDreamRoomPosition;
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private PowerUpScreen _powerUpScreen;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameObject _powerUpPortal;
    [SerializeField] private GameObject _playerTeleportPosition;
    
    void Start()
    {
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
            StartCoroutine(AfterDreamRoom());
        }
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted)) {
            _powerUpPortal.SetActive(false);
        }
    }

    private IEnumerator AfterDreamRoom() {
        SceneManager.SetActiveScene(gameObject.scene);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.isForcePushJumping = false;
        PlayerMovement.obj.jumpedWhileForcePushJumping = false;
        PlayerMovement.obj.CancelJumping();
        Player.obj.SetAnimatorLayerAndHasCape(true);        
        Player.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;
        Player.obj.PlayGetUpAnimation();
        DustParticleMgr.obj.Enabled = true;

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        AmbienceManager.obj.Play(_caveMain);

        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        Player.obj.StartAnimator();

        yield return new WaitForSeconds(4.5f);
        
        GameManager.obj.IsPauseAllowed = false;
        Time.timeScale = 0;
        _powerUpScreen.Show();
        SoundFXManager.obj.Play2D(_powerupFanfareStinger);
        while(!_powerUpScreen.PowerUpScreenCompleted) {
            yield return null;
        }
        Time.timeScale = 1;
        GameManager.obj.IsPauseAllowed = true;

        PlayerPowersManager.obj.EliCanTurnFromBlobToHuman = true;
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void TeleportToDreamRoom() {
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted))
            return;
        PlayerBlobMovement.obj.Freeze();
        AmbienceManager.obj.Stop();
        MusicManager.obj.Stop();
        StartCoroutine(TeleportToDreamRoomRoutine());
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        yield return new WaitForSeconds(1f);
        PlayerBlobMovement.obj.isGrounded = true;
        PlayerBlobMovement.obj.SetStartingOnGround();
        PlayerBlob.obj.transform.position = _playerTeleportPosition.transform.position;
        PlayerBlob.obj.SetNewPower();
        PlayerBlob.obj.FlashFor(2f);
        _powerUpPortal.GetComponent<Animator>().SetTrigger("enableFast");
        yield return new WaitForSeconds(1.5f);

        SoundFXManager.obj.Play2D(_teleportSfx);
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
}
