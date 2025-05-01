using UnityEngine;

public class SetFollowPlayerTrigger : MonoBehaviour
{
    private BoxCollider2D _collider;
    void Awake()
    {
        if(GameEventManager.obj.CaveAvatarFreed)    
        {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            CaveAvatar.obj.FollowPlayer();
            GameEventManager.obj.IsPauseAllowed = true;
            GameEventManager.obj.CaveAvatarFreed = true;
        }
    }
}
