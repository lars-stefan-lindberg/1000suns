using System.Collections;
using UnityEngine;

public class C1FreeCaveAvatarManager : MonoBehaviour
{
    [SerializeField] private Transform[] _caveAvatarFreePositions;
    [SerializeField] private Transform _finalCaveAvatarFlyPosition;
    [SerializeField] private ConversationManager _conversationManager;
    private BoxCollider2D _collider;

    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }
    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            StartCoroutine(StartCutscene());
        }
    }

    private IEnumerator StartCutscene() {
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(1f);

        Player.obj.PlayPullRoots();

        yield return new WaitForSeconds(4.8f);

        Player.obj.EndPullRoots();

        //Soot flies happily
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[0]);
        CaveAvatar.obj.SetFloatingEnabled(true);
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[1]);
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_caveAvatarFreePositions[2]);
        yield return new WaitForSeconds(1.5f);

        //Start dialogue
        _conversationManager.StartConversation();

        yield return null;
    }

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.SetTarget(_finalCaveAvatarFlyPosition);
    }
}
