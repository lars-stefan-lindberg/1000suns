using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C35ConversationTrigger : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private GameObject _fixedCamera;
    [SerializeField] private BreakableFloor _breakableFloor;
    [SerializeField] private bool _runOnConversationCompleted = true;
    [SerializeField] private bool _flipCaveAvatar = false;
    [SerializeField] private bool _isFirstConversation = false;
    [SerializeField] private Transform _deeCutsceneStartingPosition;
    [SerializeField] private SceneField _firstCaveBackground;
    [SerializeField] private SceneField _firstCaveSurfaces;

    private BoxCollider2D _collider;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;

        if(_deeCutsceneStartingPosition != null) {
            ShadowTwinMovement.obj.gameObject.tag = "Untagged"; //Hack to avoid player triggers to activate like RoomMgr and LevelEntry
            ShadowTwinPlayer.obj.gameObject.SetActive(true);
            ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinPlayer.obj.ResetAnimator();
            ShadowTwinPlayer.obj.StartAnimator();
            ShadowTwinPlayer.obj.transform.position = _deeCutsceneStartingPosition.position;
        }
    }

    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            _collider.enabled = false;
            PlayerMovement.obj.SetMovementInput(Vector2.zero);
            StartCoroutine(SetupDialogue());
        }
    }

    private IEnumerator SetupDialogue() {
        if(Player.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze();
        } else if(PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze();
            PlayerBlobMovement.obj.ToHuman();
        }

        if(_isFirstConversation) {
            _fixedCamera.SetActive(true);
            yield return new WaitForSeconds(2.4f);
            ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
            yield return new WaitForSeconds(1f);
            ShadowTwinMovement.obj.SetMovementInput(new Vector2(0, 0));
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
            ShadowTwinMovement.obj.gameObject.tag = "Player";
        } else {
            yield return new WaitForSeconds(0.5f);
        }

        if(_flipCaveAvatar) {
            CaveAvatar.obj.SetFlipX(true);
            yield return new WaitForSeconds(1.3f);
        }
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        if(_runOnConversationCompleted) {
            PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        }
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        if(_nextConversationManager != null) {
            _nextConversationManager.enabled = true;
        }
        if(_breakableFloor != null) {
            _conversationManager.CleanUp();
            StartCoroutine(BreakFloor());
        }
    }

    private IEnumerator BreakFloor() {
        GameManager.obj.IsPauseAllowed = false;

        AmbienceManager.obj.Stop();
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.Attack();
        _breakableFloor.Shake();
        yield return new WaitForSeconds(0.8f);
        _breakableFloor.Break();

        yield return new WaitForSeconds(3f);

        SceneFadeManager.obj.StartFadeOut(0.3f);

        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        yield return new WaitForSeconds(3f);

        //Set player objects inactive
        Player.obj.gameObject.SetActive(false);
        CaveAvatar.obj.gameObject.SetActive(false);
        ShadowTwinPlayer.obj.gameObject.SetActive(false);

        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        WalkableSurfacesManager.obj.RemoveAllSurfaces();

        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_firstCaveBackground));
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstCaveSurfaces));

        //Set cave timeline
        GameManager.obj.SetCaveTimeline(new CaveTimeline(CaveTimelineId.Id.Dee));
        PlayerSwitcher.obj.SwitchToDee();

        //Load first cave room
        AsyncOperation loadFirstCaveRoomOperation = SceneManager.LoadSceneAsync("Cave-1", LoadSceneMode.Additive);
        while(!loadFirstCaveRoomOperation.isDone) {
            yield return null;
        }
        Scene firstScene = SceneManager.GetSceneByName("Cave-1");
        SceneManager.SetActiveScene(firstScene);
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(firstScene);
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        //Unload current rooms
        SceneManager.UnloadSceneAsync("Cave-55");
        SceneManager.UnloadSceneAsync("Cave-56");

        yield return null;
    }
}
