using System.Collections;
using System.Linq;
using Cinemachine;
using FunkyCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToC295 : MonoBehaviour
{
    [SerializeField] private SceneField _sceneToTeleportTo;
    [SerializeField] private GameObject _lightPortal;
    [SerializeField] private CinemachineVirtualCamera _followCamera;

    void Awake()
    {
        if(PlayerPowersManager.obj.CanTurnFromBlobToHuman) {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(TeleportToC295Routine());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator TeleportToC295Routine() 
    {
        GameEventManager.obj.IsPauseAllowed = false;
        PlayerBlobMovement.obj.Freeze();

        yield return new WaitForSeconds(0.2f);
        
        SoundFXManager.obj.PlayCaveSpaceRoomTeleport();
        WhiteFadeManager.obj.StartFadeOut();

        yield return new WaitForSeconds(1f);

        LightingManager2D.Get().profile.DarknessColor = new Color(0.05f, 0.05f, 0.05f);

        Scene scene = SceneManager.GetSceneByName(_sceneToTeleportTo.SceneName);
        SceneManager.SetActiveScene(scene);
        GameObject[] sceneGameObjects = scene.GetRootGameObjects();

        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        cameraManager.ActivateAlternativeCamera();

        _followCamera.Follow = Player.obj.transform;

        GameObject playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
        Collider2D playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();
        PlayerBlob.obj.transform.position = playerSpawningCollider.transform.position - new Vector3(0, 0.5f, 0);
        PlayerBlobMovement.obj.SetStartingOnGround();
        PlayerBlobMovement.obj.isGrounded = true;
        PlayerBlobMovement.obj.CancelJumping();

        yield return new WaitForSeconds(2f);
        MusicManager.obj.PlayCaveSpaceRoomIntro();
        yield return new WaitForSeconds(1f);
        WhiteFadeManager.obj.StartFadeIn();

        Destroy(_lightPortal);

        yield return new WaitForSeconds(6f);

        cameraManager.ActivateMainCamera();

        yield return new WaitForSeconds(5f);

        PlayerBlobMovement.obj.UnFreeze();

        Destroy(gameObject);

        yield return null;
    }
}
