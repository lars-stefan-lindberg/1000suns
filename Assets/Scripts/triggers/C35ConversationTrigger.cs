using System.Collections;
using Cinemachine;
using UnityEngine;

public class C35ConversationTrigger : MonoBehaviour
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private ConversationManager _nextConversationManager;
    [SerializeField] private BreakableFloor _breakableFloor;
    [SerializeField] private GameObject _camera;
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

        yield return new WaitForSeconds(0.5f);

        if(_flipCaveAvatar) {
            CaveAvatar.obj.SetFlipX(true);
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
        GameEventManager.obj.IsPauseAllowed = false;
        PlayerStatsManager.obj.PauseTimer();
        MusicManager.obj.PlayEndSong();
        if(_camera != null) {
            _camera.SetActive(true);
            CinemachineVirtualCamera cinemachineVirtualCamera = _camera.GetComponent<CinemachineVirtualCamera>();
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
