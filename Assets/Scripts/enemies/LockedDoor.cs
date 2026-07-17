using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    private Animator _animator;
    private BoxCollider2D _collider;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;

    void Awake() {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnEnable() {
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
        _lightSprite2DFadeManager.StartFadeIn();
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerShadow(PlayerManager.obj.GetPlayerTypeFromCollision(other));
        }
    }

    [ContextMenu("PlayDeathAnimation")]
    public void PlayDeathAnimation() {
        _animator.SetTrigger("death");
        _collider.enabled = false;
        _lightSprite2DFadeManager.SetFadedInState();
        _lightSprite2DFadeManager.StartFadeOut();
        Destroy(this, 5);
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
