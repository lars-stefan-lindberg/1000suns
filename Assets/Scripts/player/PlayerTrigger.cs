using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerExited;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;
        OnPlayerEntered?.Invoke();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;
        OnPlayerExited?.Invoke();
    }
}
