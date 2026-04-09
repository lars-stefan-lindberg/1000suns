using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class ForestTent : MonoBehaviour
{
    [SerializeField] private float _waitTimeAfterShake = 2f;
    [SerializeField] private float _waitTimeAfterRumble = 1f;
    [SerializeField] private EventReference _tentShakeSfx;
    [SerializeField] private EventReference _tentRumbleSfx;
    [SerializeField] private EventReference _tentOpenSfx;
    public UnityEvent OnOpenedEvent;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();    
    }

    public void Rumble() {
        _animator.SetTrigger("rumble");
    }

    public void Shake()  {
        _animator.SetTrigger("shake");
    }

    public void PlayShakeSfx() {
        SoundFXManager.obj.PlayAtPosition(_tentShakeSfx, transform.position);
    }

    public void PlayRumbleSfx() {
        SoundFXManager.obj.PlayAtPosition(_tentRumbleSfx, transform.position);
    }

    public void PlayOpenSfx() {
        SoundFXManager.obj.PlayAtPosition(_tentOpenSfx, transform.position);
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

    public void OnRumbleFinished() {
        StartCoroutine(WaitAndShake());
    }

    private IEnumerator WaitAndOpen() {
        yield return new WaitForSeconds(_waitTimeAfterShake);
        Open();
    }

    private IEnumerator WaitAndShake() {
        yield return new WaitForSeconds(_waitTimeAfterRumble);
        Shake();
    }

    public void OnOpened() {
        OnOpenedEvent?.Invoke();
    }
}
