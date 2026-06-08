using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SootPortrait : MonoBehaviour, IPortrait
{
    [System.Serializable]
    public struct EmotionData {
        public DialogueContent.Emotion emotion;
        public Sprite faceSprite;
        public string animatorLayerName;
    }
    [SerializeField] private Animator _eyesAnimator;
    [SerializeField] private Image _face;
    [SerializeField] private List<EmotionData> _emotionMappings;
    
    [Header("Hover Effect")]
    [SerializeField] private float _hoverMaxHeight = 10f;
    [SerializeField] private float _hoverMinHeight = -10f;
    [SerializeField] private float _hoverDuration = 2.2f;
    [SerializeField] private float _hoverSlowMultiplier = 1.3f;
    [SerializeField] private float _hoverFastMultiplier = 0.7f;
    [SerializeField] private float _hoverReturnDuration = 0.5f;
    
    [Header("Cold Effect")]
    [SerializeField] private float _coldScaleTarget = 1.2f;
    [SerializeField] private float _coldScaleUpDuration = 0.5f;
    [SerializeField] private float _coldScaleDownDuration = 0.3f;

    [Header("Annoyed Effect")]
    [SerializeField] private float _annoyedShiftDistance = 30f;
    [SerializeField] private float _annoyedShiftDuration = 0.15f;
    [SerializeField] private float _annoyedReturnDuration = 0.2f;
    [SerializeField] private int _annoyedNumberOfLoops = 5;
    [SerializeField] private Ease _annoyedEase = Ease.InOutSine;
    
    [Header("Eh Effect")]
    [SerializeField] private float _ehFadeInDuration = 0.15f;
    [SerializeField] private float _ehHoldDuration = 1f;
    [SerializeField] private float _ehFadeOutDuration = 0.25f;
    
    [Header("Excited Effect")]
    [SerializeField] private float _excitedSpinDuration = 0.5f;
    [SerializeField] private Ease _excitedSpinEase = Ease.OutQuad;
    
    [Header("Happy Effect")]
    [SerializeField] private float _happyJumpPower = 30f;
    [SerializeField] private int _happyNumJumps = 1;
    [SerializeField] private float _happyJumpDuration = 0.5f;
    [SerializeField] private Ease _happyJumpEase = Ease.OutQuad;
    
    [Header("Miserable Effect")]
    [SerializeField] private float _miserableShiftDistance = 30f;
    [SerializeField] private float _miserableShiftDuration = 0.3f;
    [SerializeField] private int _miserableNumberOfLoops = 3;
    [SerializeField] private Ease _miserableEase = Ease.InOutSine;
    
    [Header("Puzzled Effect")]
    [SerializeField] private float _puzzledScaleYTarget = 1.15f;
    [SerializeField] private float _puzzledScaleUpDuration = 0.2f;
    [SerializeField] private float _puzzledScaleDownDuration = 0.25f;
    
    [Header("Stuck Effect")]
    [SerializeField] private float _stuckJumpPower = 15f;
    [SerializeField] private int _stuckNumJumps = 2;
    [SerializeField] private float _stuckJumpDuration = 0.4f;
    
    private int _currentEyesLayer = 0;
    private Dictionary<DialogueContent.Emotion, EmotionData> _emotionCache;
    private DialogueContent.Emotion _currentEmotion;
    
    public bool IsLeft { get; set; }
    
    private Image[] _portraitImages;
    private Material[] _instancedMaterials;
    private Coroutine _currentEffectCoroutine;
    private Tween _currentScaleTween;
    private Tween _horizontalShiftTween;
    private Vector3 _originalLocalPosition;
    private Vector3 _originalLocalScale;
    
    // Manual hover state
    private bool _isHovering = false;
    private float _hoverTime = 0f;
    private HoverSpeed _currentHoverSpeed = HoverSpeed.Normal;
    private Tween _hoverReturnTween;

    void Awake()
    {
        _emotionCache = new Dictionary<DialogueContent.Emotion, EmotionData>();
        foreach (var emotionData in _emotionMappings)
        {
            _emotionCache[emotionData.emotion] = emotionData;
        }
        
        _originalLocalPosition = transform.localPosition;
        _originalLocalScale = transform.localScale;
        InitializeMaterials();
        StartHovering();
    }
    
    public void CaptureOriginalScale()
    {
        _originalLocalScale = transform.localScale;
    }
    
    private void InitializeMaterials()
    {
        _portraitImages = GetComponentsInChildren<Image>();
        _instancedMaterials = new Material[_portraitImages.Length];
        
        for (int i = 0; i < _portraitImages.Length; i++)
        {
            if (_portraitImages[i].material != null)
            {
                _instancedMaterials[i] = Instantiate(_portraitImages[i].material);
                _portraitImages[i].material = _instancedMaterials[i];
            }
        }
    }

    [ContextMenu("Blink")]
    public void Blink()
    {
        _eyesAnimator.SetTrigger("blink");
    }

    [ContextMenu("DoubleBlink")]
    public void DoubleBlink()
    {
        _eyesAnimator.SetTrigger("doubleBlink");
    }

    public void SwitchEmotion(string emotion)
    {
        if (!System.Enum.TryParse(emotion, out DialogueContent.Emotion emotionEnum))
        {
            return;
        }

        if (!_emotionCache.TryGetValue(emotionEnum, out EmotionData emotionData))
        {
            return;
        }

        _face.sprite = emotionData.faceSprite;
        
        int newLayer = _eyesAnimator.GetLayerIndex(emotionData.animatorLayerName);
        if (newLayer != -1)
        {
            _eyesAnimator.SetLayerWeight(_currentEyesLayer, 0);
            _eyesAnimator.SetLayerWeight(newLayer, 1);
            _currentEyesLayer = newLayer;
        }

        _currentEmotion = emotionEnum;
    }
    
    private void ResetVFXState()
    {
        // Stop and clean up active tweens (preserve hover)
        if (_currentScaleTween != null && _currentScaleTween.IsActive())
        {
            _currentScaleTween.Kill();
        }
        
        if (_horizontalShiftTween != null && _horizontalShiftTween.IsActive())
        {
            _horizontalShiftTween.Kill();
        }
        
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
            _currentEffectCoroutine = null;
        }
        
        // Reset transform properties
        transform.localScale = _originalLocalScale;
        transform.localRotation = Quaternion.identity;
        
        // Reset X position, preserve Y for hover
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = new Vector3(_originalLocalPosition.x, currentPos.y, currentPos.z);
        
        // Reset material shader properties
        SetWiggleFadeValue(0f);
        
        // Restart hover if it's not running (handles Stuck case)
        if (!IsHovering())
        {
            StartHovering();
        }
    }
    
    public void PlayVFX(PortraitVFX vfx)
    {
        ResetVFXState();
        
        switch (vfx)
        {
            case PortraitVFX.None:
                break;
            case PortraitVFX.Annoyed:
                TriggerAnnoyedEffect();
                break;
            case PortraitVFX.Cold:
            case PortraitVFX.Evil:
                TriggerColdEffect();
                break;
            case PortraitVFX.Eh:
                TriggerEhEffect();
                break;
            case PortraitVFX.Excited:
                TriggerExcitedEffect();
                break;
            case PortraitVFX.Happy:
                TriggerHappyEffect();
                break;
            case PortraitVFX.Miserable:
                TriggerMiserableEffect();
                break;
            case PortraitVFX.Puzzled:
                TriggerPuzzledEffect();
                break;
            case PortraitVFX.Stuck:
                TriggerStuckEffect();
                break;
            default:
                Debug.LogWarning($"VFX {vfx} not implemented for SootPortrait");
                break;
        }
    }
    
    public void TriggerAnnoyedEffect()
    {
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
        }
        _currentEffectCoroutine = StartCoroutine(AnnoyedEffectCoroutine());
    }
    
    private IEnumerator AnnoyedEffectCoroutine()
    {
        float shiftDirection = IsLeft ? -1f : 1f;
        
        for (int i = 0; i < _annoyedNumberOfLoops; i++)
        {
            _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x + (_annoyedShiftDistance * shiftDirection), _annoyedShiftDuration / 2).SetEase(_annoyedEase);
            yield return _horizontalShiftTween.WaitForCompletion();
            
            _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x - (_annoyedShiftDistance * shiftDirection), _annoyedShiftDuration / 2).SetEase(_annoyedEase);
            yield return _horizontalShiftTween.WaitForCompletion();
        }
        
        _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x, _annoyedShiftDuration / 2).SetEase(Ease.InOutQuad);
        yield return _horizontalShiftTween.WaitForCompletion();
        
        _currentEffectCoroutine = null;
    }
    
    public void TriggerMiserableEffect()
    {
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
        }
        _currentEffectCoroutine = StartCoroutine(MiserableEffectCoroutine());
    }
    
    private IEnumerator MiserableEffectCoroutine()
    {
        float shiftDirection = IsLeft ? -1f : 1f;
        
        for (int i = 0; i < _miserableNumberOfLoops; i++)
        {
            _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x + (_miserableShiftDistance * shiftDirection), _miserableShiftDuration / 2).SetEase(_miserableEase);
            yield return _horizontalShiftTween.WaitForCompletion();
            
            _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x - (_miserableShiftDistance * shiftDirection), _miserableShiftDuration / 2).SetEase(_miserableEase);
            yield return _horizontalShiftTween.WaitForCompletion();
        }
        
        _horizontalShiftTween = transform.DOLocalMoveX(_originalLocalPosition.x, _miserableShiftDuration / 2).SetEase(Ease.InOutQuad);
        yield return _horizontalShiftTween.WaitForCompletion();
        
        _currentEffectCoroutine = null;
    }
    
    public void TriggerPuzzledEffect()
    {
        DOTween.Init();
        
        if (_currentScaleTween != null && _currentScaleTween.IsActive())
        {
            _currentScaleTween.Kill();
        }
        
        transform.localScale = Vector3.one;
        
        _currentScaleTween = transform.DOScaleY(_puzzledScaleYTarget, _puzzledScaleUpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScaleY(1f, _puzzledScaleDownDuration).SetEase(Ease.InQuad);
            });
    }

    public void TriggerColdEffect()
    {
        DOTween.Init();
        
        if (_currentScaleTween != null && _currentScaleTween.IsActive())
        {
            _currentScaleTween.Kill();
        }
        
        transform.localScale = Vector3.one;
        
        _currentScaleTween = transform.DOScale(Vector3.one * _coldScaleTarget, _coldScaleUpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScale(Vector3.one, _coldScaleDownDuration).SetEase(Ease.InQuad);
            });
    }
    
    public void TriggerEhEffect()
    {
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
        }
        _currentEffectCoroutine = StartCoroutine(EhEffectCoroutine());
    }
    
    private IEnumerator EhEffectCoroutine()
    {
        yield return FadeWiggleEffect(0f, 1f, _ehFadeInDuration);
        
        if (_ehHoldDuration > 0)
        {
            yield return new WaitForSeconds(_ehHoldDuration);
        }
        
        yield return FadeWiggleEffect(1f, 0f, _ehFadeOutDuration);
        
        _currentEffectCoroutine = null;
    }
    
    private IEnumerator FadeWiggleEffect(float fromValue, float toValue, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(fromValue, toValue, t);
            
            SetWiggleFadeValue(currentValue);
            
            yield return null;
        }
        
        SetWiggleFadeValue(toValue);
    }
    
    private void SetWiggleFadeValue(float value)
    {
        if (_instancedMaterials == null) return;
        
        foreach (Material mat in _instancedMaterials)
        {
            if (mat != null)
            {
                mat.SetFloat("_WiggleFade", value);
            }
        }
    }
    
    public void TriggerExcitedEffect()
    {
        DOTween.Init();
        
        if (_currentScaleTween != null && _currentScaleTween.IsActive())
        {
            _currentScaleTween.Kill();
        }
        
        float rotationDirection = IsLeft ? 360f : -360f;
        
        _currentScaleTween = transform.DORotate(new Vector3(0, 0, rotationDirection), _excitedSpinDuration, RotateMode.LocalAxisAdd)
            .SetEase(_excitedSpinEase);
    }
    
    public void TriggerHappyEffect()
    {
        DOTween.Init();
        
        StopHovering();
        
        Sequence jumpSequence = DOTween.Sequence();
        
        for (int i = 0; i < _happyNumJumps; i++)
        {
            float jumpUpDuration = _happyJumpDuration / (_happyNumJumps * 2f);
            float jumpDownDuration = _happyJumpDuration / (_happyNumJumps * 2f);
            
            jumpSequence.Append(transform.DOLocalMoveY(_originalLocalPosition.y + _happyJumpPower, jumpUpDuration).SetEase(Ease.OutQuad));
            jumpSequence.Append(transform.DOLocalMoveY(_originalLocalPosition.y, jumpDownDuration).SetEase(Ease.InQuad));
        }
        
        jumpSequence.OnComplete(() => {
            StartHovering();
        });
        
        _currentScaleTween = jumpSequence;
    }
    
    public void TriggerStuckEffect()
    {
        DOTween.Init();
        
        StopHovering();
        
        Sequence jumpSequence = DOTween.Sequence();
        
        for (int i = 0; i < _stuckNumJumps; i++)
        {
            float jumpUpDuration = _stuckJumpDuration / (_stuckNumJumps * 2f);
            float jumpDownDuration = _stuckJumpDuration / (_stuckNumJumps * 2f);
            
            jumpSequence.Append(transform.DOLocalMoveY(_originalLocalPosition.y + _stuckJumpPower, jumpUpDuration).SetEase(Ease.OutQuad));
            jumpSequence.Append(transform.DOLocalMoveY(_originalLocalPosition.y, jumpDownDuration).SetEase(Ease.InQuad));
        }
        
        _currentScaleTween = jumpSequence;
    }
    
    void Update()
    {
        if (_isHovering)
        {
            UpdateHover();
        }
    }
    
    private void UpdateHover()
    {
        float duration = _hoverDuration * GetHoverDurationMultiplier(_currentHoverSpeed);
        
        _hoverTime += Time.deltaTime;
        
        float normalizedTime = (_hoverTime % duration) / duration;
        float pingPongValue = Mathf.PingPong(normalizedTime * 2f, 1f);
        float easedValue = EaseInOutSine(pingPongValue);
        
        float hoverRange = _hoverMaxHeight - _hoverMinHeight;
        float newY = _originalLocalPosition.y + _hoverMinHeight + (easedValue * hoverRange);
        
        Vector3 pos = transform.localPosition;
        pos.y = newY;
        transform.localPosition = pos;
    }
    
    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }
    
    private float GetHoverDurationMultiplier(HoverSpeed speed)
    {
        return speed switch
        {
            HoverSpeed.Slow => _hoverSlowMultiplier,
            HoverSpeed.Fast => _hoverFastMultiplier,
            _ => 1.0f
        };
    }
    
    public void StartHovering()
    {
        if (_hoverReturnTween != null && _hoverReturnTween.IsActive())
        {
            _hoverReturnTween.Kill();
        }
        
        transform.localPosition = _originalLocalPosition;
        _hoverTime = 0f;
        _isHovering = true;
    }
    
    public void StopHovering()
    {
        _isHovering = false;
        
        if (_hoverReturnTween != null && _hoverReturnTween.IsActive())
        {
            _hoverReturnTween.Kill();
        }
        
        _hoverReturnTween = transform.DOLocalMove(_originalLocalPosition, _hoverReturnDuration)
            .SetEase(Ease.OutQuad);
    }
    
    public void PauseHovering()
    {
        _isHovering = false;
    }
    
    public void ResumeHovering()
    {
        _isHovering = true;
    }
    
    public bool IsHovering()
    {
        return _isHovering;
    }
    
    public void SetHoverSpeed(HoverSpeed speed)
    {
        if (!_isHovering)
        {
            _currentHoverSpeed = speed;
            return;
        }
        
        float oldDuration = _hoverDuration * GetHoverDurationMultiplier(_currentHoverSpeed);
        float oldNormalizedTime = (_hoverTime % oldDuration) / oldDuration;
        
        float newDuration = _hoverDuration * GetHoverDurationMultiplier(speed);
        _hoverTime = oldNormalizedTime * newDuration;
        
        _currentHoverSpeed = speed;
    }

    public Animator GetEyesAnimator() {
        return _eyesAnimator;
    }
}
