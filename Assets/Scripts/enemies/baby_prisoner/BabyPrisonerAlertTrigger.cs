using UnityEngine;

public class BabyPrisonerAlertTrigger : MonoBehaviour
{
    public BabyPrisoner babyPrisoner;
    public float playerFreezeTime = 3f;
    private BoxCollider2D _boxCollider;

    void Awake() {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player")) {
            if(playerFreezeTime > 0) {
                PlayerMovement.obj.Freeze(playerFreezeTime);
            }
            babyPrisoner.Hide(transform.position);
            _boxCollider.enabled = false;
        }
    }
}
