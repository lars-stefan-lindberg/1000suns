using System.Collections;
using DG.Tweening;
using FMODUnity;
using UnityEngine;

public class CaveAvatarRootsManager : MonoBehaviour
{
    [SerializeField] private Animator _rootsAnimator;
    [SerializeField] private EventReference _rootsPulled;
    [SerializeField] private GameEventId _sootFreed;

    public float duration = 3f;
    private float timer = 0;
    private bool _startTimer = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(GameManager.obj.HasEvent(_sootFreed))
            return;
        if(collision.gameObject.CompareTag("Player")) {
            _startTimer = true;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    public void Stop()
    {
        _startTimer = false;
        timer = 0;
    }

    void FixedUpdate()
    {
        if(_startTimer) {
            timer += Time.fixedDeltaTime;
            if(timer > duration) {
                StartCoroutine(Struggle());
                timer = 0;
            }
        }
    }

    private IEnumerator Struggle() {
        SoundFXManager.obj.PlayAtPosition(_rootsPulled, Camera.main.transform.position);
        CaveAvatar.obj.NudgeUpwards();
        if(_rootsAnimator != null)
            _rootsAnimator.SetTrigger("expand");
        yield return new WaitForSeconds(1f);
        SoundFXManager.obj.PlayAtPosition(_rootsPulled, Camera.main.transform.position);
        CaveAvatar.obj.NudgeUpwards();
        if(_rootsAnimator != null)
            _rootsAnimator.SetTrigger("expand");
        yield return null;
    }
}
