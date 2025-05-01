using System.Collections;
using UnityEngine;

public class CaveRootsTrap : MonoBehaviour
{
    public static CaveRootsTrap obj;

    public float _fadeOutSpeed = 1f;
    [SerializeField] private ParticleSystem _particleSystem;
    private SpriteRenderer _renderer;

    void Awake()
    {
        obj = this;
    }

    void Start()
    {
        if(GameEventManager.obj.CaveAvatarFreed) {
            Destroy(gameObject);
            return;
        }
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void StartBreaking() {
        StartCoroutine(Break());
    }

    private IEnumerator Break() {
        _particleSystem.Emit(10);

        //Fade out sprite
        while(_renderer.color.a > 0f) {
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, _renderer.color.a - Time.deltaTime * _fadeOutSpeed);
            yield return null;
        }

        Destroy(gameObject);

        yield return null;
    }

    void OnDestroy() {
        obj = null;
    }
}
