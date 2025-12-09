using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    [SerializeField] private GameObject _door;
    [SerializeField] private bool _isPermanent = false;
    private bool _isTriggered = false;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Block"))
        {
            _isTriggered = true;
            OpenDoor();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(_isPermanent) return;
        if(collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Block"))
        {
            _isTriggered = false;
            CloseDoor();
        }
    }

    private void OpenDoor() {
        if(_isTriggered)
        {
            SoundFXManager.obj.PlayBabyPrisonerIdle(_door.transform);
            _door.SetActive(false);
        }
    }

    private void CloseDoor() {
        SoundFXManager.obj.PlayBabyPrisonerIdle(_door.transform);
        _door.SetActive(true);
    }
}
