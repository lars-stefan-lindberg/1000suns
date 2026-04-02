using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ForestTent : MonoBehaviour
{
    [SerializeField] private float _waitTimeAfterShake = 2f;
    public UnityEvent OnOpenedEvent;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();    
    }

    public void Shake()  {
        _animator.SetTrigger("shake");
    }

    public void Open() {
        _animator.SetTrigger("open");
    }

    public void Close() {
        _animator.SetTrigger("close");
    }

    public void StopAnimator() {
        _animator.StopPlayback();
    }

    /* Animator event methods*/
    public void OnShakeFinished() {
        StartCoroutine(WaitAndOpen());
    }

    private IEnumerator WaitAndOpen() {
        yield return new WaitForSeconds(_waitTimeAfterShake);
        Open();
    }

    public void OnOpened() {
        OnOpenedEvent?.Invoke();
    }
}
