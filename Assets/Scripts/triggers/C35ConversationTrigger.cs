using System.Collections;
using Cinemachine;
using UnityEngine;

public class C35ConversationTrigger : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private DreadbinderEndRoom _dreadBinder;
    [SerializeField] private BreakableFloor _breakableFloor;
    [SerializeField] private GameObject _fixedCamera;
    [SerializeField] private GameObject _followCamera;
    [SerializeField] private bool _runOnConversationCompleted = true;
    [SerializeField] private bool _flipCaveAvatar = false;
    private BoxCollider2D _collider;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
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

        if(_fixedCamera != null) {
            //If fixed camera is set, this is dialogue part 1
            _fixedCamera.SetActive(true);
            CinemachineVirtualCamera cinemachineVirtualCamera = _fixedCamera.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.enabled = true;
            yield return new WaitForSeconds(1.8f);
            _dreadBinder.ChangeSpriteThenMoveRight();
            yield return new WaitForSeconds(2.4f);
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
            StartCoroutine(BreakFloor());
        }
    }

    private IEnumerator BreakFloor() {
        GameManager.obj.IsPauseAllowed = false;
        PlayerStatsManager.obj.PauseTimer();
        MusicManager.obj.PlayEndSong();
        AmbienceManager.obj.FadeOutAmbienceSource1(1f);
        if(_followCamera != null) {
            _followCamera.SetActive(true);
            CinemachineVirtualCamera cinemachineVirtualCamera = _followCamera.GetComponent<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.Follow = Player.obj.transform;
            cinemachineVirtualCamera.enabled = true;
        }
        yield return new WaitForSeconds(2f);
        CaveAvatar.obj.Attack();
        _breakableFloor.Shake();
        yield return new WaitForSeconds(0.8f);
        _breakableFloor.Break();

        yield return new WaitForSeconds(1f);
    }
}
