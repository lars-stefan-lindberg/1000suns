using UnityEngine;
using UnityEngine.SceneManagement;

public class SetFollowPlayerTrigger : MonoBehaviour
{
    [SerializeField] private GameEventId _event;
    private BoxCollider2D _collider;
    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            CaveAvatar.obj.FollowPlayer();
            GameManager.obj.IsPauseAllowed = true;
            GameManager.obj.RegisterEvent(_event);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
