using UnityEngine;

public class MirrorRoomStingerTrigger : MonoBehaviour
{
    private BoxCollider2D _collider;
    
    void Start() {
        _collider = GetComponent<BoxCollider2D>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            if(GameManager.obj.MirrorConversationEnded) {
                return;
            }
            //Deprecated, use FMOD event reference instead
            //SoundFXManager.obj.PlayCaveAvatarEvilEyesTransition();
            _collider.enabled = false;
        }
    }
}
