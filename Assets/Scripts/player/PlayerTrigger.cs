using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
    [SerializeField] private bool _deactivateOnTrigger = false;
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerExited;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;
        OnPlayerEntered?.Invoke();
        if(_deactivateOnTrigger)
            GetComponent<Collider2D>().enabled = false;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;
        OnPlayerExited?.Invoke();
    }
}
