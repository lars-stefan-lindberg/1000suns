using UnityEngine;

public class OpenExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _levelExitDoor;
    private int _playerCount = 0;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            _playerCount++;

            if(PlayerManager.obj.IsSeparated && _playerCount == 2) 
            {
                OpenDoor();
            } else if(!PlayerManager.obj.IsSeparated && _playerCount == 1) 
            {
                OpenDoor();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            _playerCount--;
        }
    }

    private void OpenDoor() {
        SoundFXManager.obj.PlayBrokenFloorReappear(transform);
        _levelExitDoor.SetActive(false);
    }
}
