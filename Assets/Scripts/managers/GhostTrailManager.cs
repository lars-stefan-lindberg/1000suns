using DG.Tweening;
using UnityEngine;

public class GhostTrailManager : MonoBehaviour
{
    public static GhostTrailManager obj;
    [SerializeField] private SpriteRenderer _playerRenderer;
    [SerializeField] private Color _trailColor;
    [SerializeField] private Color _fadeColor;
    [SerializeField] private float _fadeTime;
    [SerializeField] private float _ghostInterval;
    private Transform _transform;

    void Awake() {
        obj = this;
        _transform = transform;
    }

    public void ShowGhosts() {
        Sequence s = DOTween.Sequence();

        for (int i = 0; i < _transform.childCount; i++)
        {
            Transform currentGhost = _transform.GetChild(i);
            s.AppendCallback(()=> currentGhost.position = Player.obj.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = _playerRenderer.flipX);
            s.AppendCallback(()=>currentGhost.GetComponent<SpriteRenderer>().sprite = _playerRenderer.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(_trailColor, 0));
            s.AppendCallback(() => FadeGhost(currentGhost.GetComponent<SpriteRenderer>()));
            s.AppendInterval(_ghostInterval);
        }
    }

    private void FadeGhost(SpriteRenderer ghostRenderer) {
        ghostRenderer.material.DOKill();
        ghostRenderer.material.DOColor(_fadeColor, _fadeTime);
    }

    void OnDestroy() {
        obj = null;
    }
}
