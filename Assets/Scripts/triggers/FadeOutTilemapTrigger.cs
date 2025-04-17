using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FadeOutTilemap : MonoBehaviour
{
    [SerializeField] private Animator _visibleLayerAnimator;
    [SerializeField] private bool _playRevealSound = true;
    private BoxCollider2D _collider;
    private Tilemap _tilemap;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
        _tilemap = GetComponentInChildren<Tilemap>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            if(_visibleLayerAnimator != null)
                _visibleLayerAnimator.SetTrigger("reveal");
            else {
                DOTween.To(() => _tilemap.color.a, x => _tilemap.color = new Color(_tilemap.color.r, _tilemap.color.g, _tilemap.color.b, x), 0, 1);
            }
            if(_playRevealSound)
                SoundFXManager.obj.PlayRevealSecret(transform);
            _collider.enabled = false;
            Destroy(gameObject, 5);
        }
    }
}
