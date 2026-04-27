using UnityEngine;
using DG.Tweening;

public class IntroEarthCloud : MonoBehaviour
{
    [Header("Scale Animation")]
    [SerializeField] private float _maxScaleX = 1.1f;
    [SerializeField] private float _maxScaleY = 1.1f;
    [SerializeField] private float _scaleUpDuration = 2f;
    [SerializeField] private float _scaleDownDuration = 2f;
    [SerializeField] private float _offsetTime = 0f;

    private Vector3 _originalScale;
    private Sequence _scaleSequence;

    private void Start()
    {
        _originalScale = transform.localScale;

        StartScaleLoop();
    }

    private void StartScaleLoop()
    {
        _scaleSequence = DOTween.Sequence();
        
        Vector3 maxScale = new Vector3(
            _originalScale.x * _maxScaleX,
            _originalScale.y * _maxScaleY,
            _originalScale.z
        );

        if (_offsetTime > 0)
        {
            _scaleSequence.AppendInterval(_offsetTime);
        }

        _scaleSequence.Append(transform.DOScale(maxScale, _scaleUpDuration).SetEase(Ease.InOutSine));
        _scaleSequence.Append(transform.DOScale(_originalScale, _scaleDownDuration).SetEase(Ease.InOutSine));
        _scaleSequence.SetLoops(-1);
    }

    private void OnDestroy()
    {
        _scaleSequence?.Kill();
    }
}
