using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Crown : MonoBehaviour
{
    [SerializeField] private GameEventId _crownPicked;
    [SerializeField] private float _hoverMinHeight = -0.2f;
    [SerializeField] private float _hoverMaxHeight = 0.2f;
    [SerializeField] private float _hoverSpeed = 1f;
    
    private Vector3 _originalPosition;
    private Tweener _hoverTween;

    void Awake() {
        if(GameManager.obj.HasEvent(_crownPicked)) {
            Destroy(gameObject, 3);
        }
    }

    void Start() {
        _originalPosition = transform.localPosition;
        StartHover();
    }
    
    private void StartHover() {
        _hoverTween?.Kill();
        
        float distance = _hoverMaxHeight - _hoverMinHeight;
        float duration = distance / _hoverSpeed;
        
        _hoverTween = transform.DOLocalMoveY(_originalPosition.y + _hoverMaxHeight, duration / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopHover() {
        StartCoroutine(StopHoverCoroutine());
    }

    private IEnumerator StopHoverCoroutine() {
        _hoverTween?.Kill();
        
        // float returnDuration = 0.5f;
        // transform.DOLocalMove(_originalPosition, returnDuration).SetEase(Ease.OutQuad);
        
        //yield return new WaitForSeconds(returnDuration);
        yield return null;
    }
    
    private void OnDestroy() {
        _hoverTween?.Kill();
    }
}
