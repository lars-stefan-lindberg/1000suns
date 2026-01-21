using System.Collections;
using UnityEngine;

public class CaveAvatarRootsManager : MonoBehaviour
{
    [SerializeField] private Animator _rootsAnimator;

    public float duration = 3f;
    private float timer = 0;
    private bool _startTimer = false;
    private BoxCollider2D _collider;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            _startTimer = true;
        }
    }

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
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
        SoundFXManager.obj.PlayMushroomSmallRattle(Camera.main.transform);
        CaveAvatar.obj.NudgeUpwards();
        if(_rootsAnimator != null)
            _rootsAnimator.SetTrigger("expand");
        yield return new WaitForSeconds(1f);
        SoundFXManager.obj.PlayMushroomSmallRattle(Camera.main.transform);
        CaveAvatar.obj.NudgeUpwards();
        if(_rootsAnimator != null)
            _rootsAnimator.SetTrigger("expand");
        yield return null;
    }
}
