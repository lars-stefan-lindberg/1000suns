using UnityEngine;

public class BabyPrisonerAlertTrigger : MonoBehaviour
{
    public BabyPrisoner babyPrisoner;
    public float playerFreezeTime = 3f;
    public AlertType alertType = AlertType.Hide;
    private BoxCollider2D _boxCollider;

    public enum AlertType {
        Hide,
        Alert
    }

    void Awake() {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player")) {
            if(playerFreezeTime > 0) {
                PlayerMovement.obj.Freeze(playerFreezeTime);
            }
            if(alertType == AlertType.Hide) {
                babyPrisoner.Hide(transform.position);
            } else if(alertType == AlertType.Alert) {
                babyPrisoner.Alert(transform.position);
            }
            _boxCollider.enabled = false;
        }
    }
}
