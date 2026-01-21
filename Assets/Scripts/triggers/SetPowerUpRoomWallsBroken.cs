using UnityEngine;

public class SetPowerUpRoomWallsBroken : MonoBehaviour
{
    private bool _hasBeenActivated = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenActivated) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            GameManager.obj.PowerUpRoomCompletedWallBreak = true;
            _hasBeenActivated = true;
        }
    }   
}
