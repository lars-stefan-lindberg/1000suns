using System.Collections;
using UnityEngine;

public class UmmaraEyesManager : MonoBehaviour
{
    [SerializeField] private float _blinkInterval = 5f;
    [SerializeField] private Animator _leftEyeAnimator;
    [SerializeField] private Animator _rightEyeAnimator;
    
    private Animator _animator;
    private Coroutine _blinkCoroutine;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    [ContextMenu("Activate")]
    public void Activate() {
        if (_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
        }
        Blink();
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void Deactivate() {
        if (_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
        _animator.enabled = false;
        StartCoroutine(DeactivateCoroutine());
    }

    private IEnumerator DeactivateCoroutine() {
        AnimatorStateInfo stateInfo = _leftEyeAnimator.GetCurrentAnimatorStateInfo(0);
        float remainingTime = (1f - stateInfo.normalizedTime) * stateInfo.length;
        yield return new WaitForSeconds(remainingTime);
        
        _leftEyeAnimator.enabled = false;
        _rightEyeAnimator.enabled = false;
    }

    private void Blink() {
        _animator.SetTrigger("blink");
    }

    private IEnumerator BlinkRoutine() {
        while (true) {
            yield return new WaitForSeconds(_blinkInterval);
            Blink();
        }
    }
}
