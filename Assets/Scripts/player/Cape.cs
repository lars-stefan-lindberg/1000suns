using System.Collections;
using UnityEngine;

public class Cape : MonoBehaviour
{
    [SerializeField] private GameEventId _capePicked;
    private Animator _animator;

    void Awake() {
        if(GameManager.obj.HasEvent(_capePicked)) {
            Destroy(gameObject, 3);
        }
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
    }

    public void StopHover() {
        StartCoroutine(StopHoverCoroutine());
    }

    public void StartAnimation() {
        _animator.enabled = true;
    }


    private IEnumerator StopHoverCoroutine() {
        //TODO: return hover to start pos
        yield return null;
    }
}
