using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave41RoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _eliStartPosition;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private Animator _backgroundAnimator;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private SceneField _nextScene;

    private float _sootDistanceToElevator;
    private float _eliDistanceToElevator;
    private bool _moveSoot = false;

    void Start() {
        PlayerMovement.obj.Freeze();
        SceneFadeManager.obj.SetFadedOutState();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
        Player.obj.transform.position = _eliStartPosition.transform.position;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;

        CaveAvatar.obj.IsFollowingPlayer = false;
        CaveAvatar.obj.SetPosition(_sootStartPosition.position);
        CaveAvatar.obj.SetFlipX(true);

        SceneManager.SetActiveScene(gameObject.scene);

        _sootDistanceToElevator = _sootStartPosition.position.y - _elevator.transform.position.y;
        _eliDistanceToElevator = Player.obj.transform.position.y - _elevator.transform.position.y;

        StartCoroutine(StartScene());
    }

    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void Update() {
        if(_moveSoot) {
            CaveAvatar.obj.SetPosition(new Vector2(CaveAvatar.obj.transform.position.x, _elevator.transform.position.y + _sootDistanceToElevator), false);
            //Player.obj.transform.position = new Vector2(Player.obj.transform.position.x, _elevator.transform.position.y + _eliDistanceToElevator);
        }
    }

    private IEnumerator StartScene() {
        //Give some time to transition from previous scene
        yield return new WaitForSeconds(1.5f);
        SceneFadeManager.obj.StartFadeIn(0.8f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;
    
        yield return new WaitForSeconds(2f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        StartCoroutine(OnConversationCompletedCoroutine());
    }

    private IEnumerator OnConversationCompletedCoroutine() {
        _moveSoot = true;
        _elevator.StartMoving();
        yield return new WaitForSeconds(1f);
        //Stop animator
        _backgroundAnimator.enabled = false;
        yield return new WaitForSeconds(3f);
        Player.obj.gameObject.SetActive(false);
        _elevator.StopAbruptly();
        SceneFadeManager.obj.StartFadeOut(0.8f);
        
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        LevelManager.obj.LoadAdjacentRooms(SceneManager.GetSceneByName(_nextScene));

        SceneManager.UnloadSceneAsync(_thisScene);
    }
}
