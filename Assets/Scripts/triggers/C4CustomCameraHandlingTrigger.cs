using UnityEngine;

public class C4CustomCameraHandlingTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _customCamera;
    private BoxCollider2D _boxCollider2D;
    private bool _isTriggered = false;

    void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        if(GameManager.obj.CapePicked || GameManager.obj.CapeRoomZoomCompleted) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !_isTriggered) {
            if(!_customCamera.activeSelf) {
                C4CustomCameraHandling c4CustomCameraHandling = GetComponent<C4CustomCameraHandling>();
                c4CustomCameraHandling.HandleCamera();
                _boxCollider2D.enabled = false;
            }
        }
    }
}
