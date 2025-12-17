using UnityEngine;

public class MultiDoorSwitch : MonoBehaviour
{
    public bool IsTriggered { get; private set; }
    private int _objectEnteredCount = 0;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Block"))
        {
            _objectEnteredCount++;
            IsTriggered = true;
            SoundFXManager.obj.PlayBabyPrisonerIdle(transform);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Enemy") || collision.CompareTag("Block"))
        {
            _objectEnteredCount--;
            if (_objectEnteredCount == 0)
            {
                IsTriggered = false;
                SoundFXManager.obj.PlayBabyPrisonerIdle(transform);
            }
        }
    }
}
