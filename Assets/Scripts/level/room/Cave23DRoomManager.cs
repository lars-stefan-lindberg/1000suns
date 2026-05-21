using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave23DRoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _deeSpawnPoint;
    [SerializeField] private SpawnPoint _eliSpawnPoint;
    [SerializeField] private GameObject _fixedCamera;
    [SerializeField] private GameObject _closingWall1;
    [SerializeField] private GameObject _closingWall2;
    [SerializeField] private ParticleSystem _eliParticles;
    [SerializeField] private ParticleSystem _deeParticles;
    [SerializeField] private AmbienceTrack _capeRoomAmbience;
    [SerializeField] private EventReference _introStinger;
    [SerializeField] private EventReference _teleport;
    [SerializeField] private SceneField _teleportBackToScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private GameEventId _dreamRoomCompleted;
    
    [Header("Pull Movement Settings")]
    [SerializeField] private float _pullAcceleration = 2f;
    [SerializeField] private float _pullMaxSpeed = 5f;
    [SerializeField] private float _touchDistance = 0.5f;

    void Start() {
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.transform.position = _eliSpawnPoint.transform.position;
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetNewPower();

        ShadowTwinMovement.obj.gameObject.tag = "Untagged"; //Hack to avoid player triggers to activate like RoomMgr and LevelEntry
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(false);
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinPlayer.obj.ResetAnimator();
        ShadowTwinPlayer.obj.StartAnimator();
        ShadowTwinPlayer.obj.transform.position = _deeSpawnPoint.transform.position;

        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerPowersManager.obj.EliCanForcePush = false;        
        DustParticleMgr.obj.Enabled = false;
        AmbienceManager.obj.Play(_capeRoomAmbience);
        StartCoroutine(TransitionIntoRoom());
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
        SceneFadeManager.obj.StartFadeIn(0.5f);
        SoundFXManager.obj.Play2D(_introStinger);
        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();
    }

    void Update() {
        if (PlayerMovement.obj != null && ShadowTwinMovement.obj != null) {
            Vector2 eliInput = PlayerMovement.obj.GetMovementInput();
            Vector2 mirroredInput = new Vector2(-eliInput.x, eliInput.y);
            ShadowTwinMovement.obj.SetMovementInput(mirroredInput);
            
            bool eliJumpHeld = PlayerMovement.obj.GetJumpHeldInput();
            ShadowTwinMovement.obj.SimulateJumpInput(eliJumpHeld, Time.time);
        }
    }

    public void ChangeCamera() {
        StartCoroutine(ChangeCameraCoroutine());
    }

    private IEnumerator ChangeCameraCoroutine() {
        PlayerMovement.obj.Freeze();
        _fixedCamera.SetActive(true);
        _closingWall1.SetActive(true);
        _closingWall2.SetActive(true);
        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.UnFreeze();
    }

    public void StartCutscene() {
        StartCoroutine(StartCutsceneCoroutine());
    }

    private IEnumerator StartCutsceneCoroutine() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(2f);
        CameraShakeManager.obj.ForcePushShake();
        Player.obj.StartBeingPulled();
        ShadowTwinPlayer.obj.StartBeingPulled();
        _eliParticles.gameObject.SetActive(true);
        _deeParticles.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        Player.obj.StartContrastFade();
        ShadowTwinPlayer.obj.StartContrastFade();
        

        float eliSpeed = 0f;
        float deeSpeed = 0f;
        
        while (true) {
            Vector3 eliPos = Player.obj.transform.position;
            Vector3 deePos = ShadowTwinPlayer.obj.transform.position;
            float horizontalDistance = Mathf.Abs(deePos.x - eliPos.x);
            
            if (horizontalDistance <= _touchDistance) {
                break;
            }
            
            eliSpeed = Mathf.Min(eliSpeed + _pullAcceleration * Time.deltaTime, _pullMaxSpeed);
            deeSpeed = Mathf.Min(deeSpeed + _pullAcceleration * Time.deltaTime, _pullMaxSpeed);
            
            float directionToCenter = Mathf.Sign(deePos.x - eliPos.x);
            Vector3 eliMovement = new Vector3(directionToCenter * eliSpeed * Time.deltaTime, 0, 0);
            Vector3 deeMovement = new Vector3(-directionToCenter * deeSpeed * Time.deltaTime, 0, 0);
            
            Player.obj.transform.position += eliMovement;
            ShadowTwinPlayer.obj.transform.position += deeMovement;
            _eliParticles.transform.position += eliMovement;
            _deeParticles.transform.position += deeMovement;
            
            yield return null;
        }
        _eliParticles.Stop();
        _deeParticles.Stop();
        SoundFXManager.obj.Play2D(_teleport);
        SceneFadeManager.obj.StartWhiteFadeOut(0.8f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        Player.obj.StopBeingPulled();
        ShadowTwinPlayer.obj.StopBeingPulled();
        Player.obj.ResetContrast();
        ShadowTwinPlayer.obj.ResetContrast();
        DustParticleMgr.obj.Enabled = true;
        Player.obj.SetAnimatorLayerAndHasCape(true);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        PlayerPowersManager.obj.EliCanForcePush = true;  

        //Before we load the room to teleport back to, make sure state is updated so we don't teleport back
        GameManager.obj.RegisterEvent(_dreamRoomCompleted);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_teleportBackToScene, LoadSceneMode.Additive);

        while(!asyncOperation.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
    }
}
