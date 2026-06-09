using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EliPortrait : MonoBehaviour, IPortrait
{
    [System.Serializable]
    public struct EmotionData {
        public DialogueContent.Emotion emotion;
        public Sprite faceSprite;
        public string animatorLayerName;
    }
    [SerializeField] private Image _face;
    [SerializeField] private GameObject _cape;
    [SerializeField] private Animator _eyesAnimator;
    [SerializeField] private List<EmotionData> _emotionMappings;
    
    [Header("Visual Effects")]
    [SerializeField] private float _poisonEffectDuration = 1f;
    [SerializeField] private float _surpriseScaleTarget = 1.2f;
    [SerializeField] private float _surpriseScaleUpDuration = 0.15f;
    [SerializeField] private float _surpriseScaleDownDuration = 0.2f;
    [SerializeField] private float _angryShakeStrength = 10f;
    [SerializeField] private float _angryShakeDuration = 0.3f;
    [SerializeField] private int _angryShakeVibrato = 10;
    [SerializeField] private float _veryAngryShakeStrength = 25f;
    [SerializeField] private float _veryAngryShakeDuration = 0.5f;
    [SerializeField] private int _veryAngryShakeVibrato = 20;
    [SerializeField] private float _puzzledScaleYTarget = 1.15f;
    [SerializeField] private float _puzzledScaleUpDuration = 0.2f;
    [SerializeField] private float _puzzledScaleDownDuration = 0.25f;
    [SerializeField] private float _annoyedShiftDistance = 30f;
    [SerializeField] private float _annoyedShiftDuration = 0.2f;
    [SerializeField] private float _annoyedReturnDuration = 0.25f;
    
    private int _currentEyesLayer = 0;
    private Dictionary<DialogueContent.Emotion, EmotionData> _emotionCache;
    private DialogueContent.Emotion _currentEmotion;
    
    public bool IsLeft { get; set; }
    
    private Image[] _portraitImages;
    private Material[] _instancedMaterials;
    private Coroutine _currentEffectCoroutine;
    private Tween _currentScaleTween;
    private Vector3 _originalLocalPosition;
    private Vector3 _originalLocalScale;

    void Awake()
    {
        _emotionCache = new Dictionary<DialogueContent.Emotion, EmotionData>();
        foreach (var emotionData in _emotionMappings)
        {
            _emotionCache[emotionData.emotion] = emotionData;
        }
        
        if(Player.obj.GetHasCape()) {
            _cape.SetActive(true);
        } else {
            _cape.SetActive(false);
        }

        _originalLocalPosition = transform.localPosition;
        _originalLocalScale = transform.localScale;
        InitializeMaterials();
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
        SwitchFaceExpression(emotion);
        SwitchEyesExpression(emotion);
    }

    public void SwitchFaceExpression(string expression)
    {
        if (!System.Enum.TryParse(expression, out DialogueContent.Emotion emotionEnum))
        {
            return;
        }

        if (!_emotionCache.TryGetValue(emotionEnum, out EmotionData emotionData))
        {
            return;
        }

        _face.sprite = emotionData.faceSprite;
        _currentEmotion = emotionEnum;
    }

    public void SwitchEyesExpression(string expression)
    {
        if (!System.Enum.TryParse(expression, out DialogueContent.Emotion emotionEnum))
        {
            return;
        }

        if (!_emotionCache.TryGetValue(emotionEnum, out EmotionData emotionData))
        {
            return;
        }

        int newLayer = _eyesAnimator.GetLayerIndex(emotionData.animatorLayerName);
        if (newLayer != -1)
        {
            _eyesAnimator.SetLayerWeight(_currentEyesLayer, 0);
            _eyesAnimator.SetLayerWeight(newLayer, 1);
            _currentEyesLayer = newLayer;
        }
    }
    
    [ContextMenu("Test Poison Effect")]
    public void TestPoisonEffect()
    {
        TriggerPoisonEffect(_poisonEffectDuration);
    }
    
    public void TriggerPoisonEffect(float duration)
    {
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
        }
        _currentEffectCoroutine = StartCoroutine(PoisonEffectCoroutine(duration));
    }
    
    private IEnumerator PoisonEffectCoroutine(float duration)
    {
        float fadeInDuration = 0.2f;
        float fadeOutDuration = 0.3f;
        float holdDuration = Mathf.Max(0, duration - fadeInDuration - fadeOutDuration);
        
        yield return FadePoisonEffect(0f, 1f, fadeInDuration);
        
        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }
        
        yield return FadePoisonEffect(1f, 0f, fadeOutDuration);
        
        _currentEffectCoroutine = null;
    }
    
    private IEnumerator FadePoisonEffect(float fromValue, float toValue, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(fromValue, toValue, t);
            
            SetPoisonFadeValue(currentValue);
            
            yield return null;
        }
        
        SetPoisonFadeValue(toValue);
    }
    
    private void SetPoisonFadeValue(float value)
    {
        if (_instancedMaterials == null) return;
        
        foreach (Material mat in _instancedMaterials)
        {
            if (mat != null)
            {
                mat.SetFloat("_PoisonFade", value);
            }
        }
    }
    
    public void TriggerSurpriseEffect()
    {
        DOTween.Init();
        
        transform.DOKill();
        transform.localScale = Vector3.one;
        
        _currentScaleTween = transform.DOScale(Vector3.one * _surpriseScaleTarget, _surpriseScaleUpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScale(Vector3.one, _surpriseScaleDownDuration).SetEase(Ease.InQuad);
            });
    }
    
    public void TriggerAngryEffect()
    {
        DOTween.Init();
        
        transform.DOKill();
        
        _currentScaleTween = transform.DOShakePosition(_angryShakeDuration, _angryShakeStrength, _angryShakeVibrato);
    }
    
    public void TriggerVeryAngryEffect()
    {
        DOTween.Init();
        
        transform.DOKill();
        
        _currentScaleTween = transform.DOShakePosition(_veryAngryShakeDuration, _veryAngryShakeStrength, _veryAngryShakeVibrato);
    }
    
    public void TriggerPuzzledEffect()
    {
        DOTween.Init();
        
        transform.DOKill();
        transform.localScale = Vector3.one;
        
        _currentScaleTween = transform.DOScaleY(_puzzledScaleYTarget, _puzzledScaleUpDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOScaleY(1f, _puzzledScaleDownDuration).SetEase(Ease.InQuad);
            });
    }
    
    public void TriggerAnnoyedEffect()
    {
        DOTween.Init();
        
        transform.DOKill();
        
        Vector3 originalPosition = transform.localPosition;
        float shiftDirection = IsLeft ? -1f : 1f;
        Vector3 targetPosition = originalPosition + new Vector3(_annoyedShiftDistance * shiftDirection, 0, 0);
        
        _currentScaleTween = transform.DOLocalMove(targetPosition, _annoyedShiftDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                transform.DOLocalMove(originalPosition, _annoyedReturnDuration).SetEase(Ease.InQuad);
            });
    }
    
    private void ResetVFXState()
    {
        // Stop and clean up active tweens
        if (_currentScaleTween != null && _currentScaleTween.IsActive())
        {
            _currentScaleTween.Kill();
        }
        
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
            _currentEffectCoroutine = null;
        }
        
        // Reset transform properties
        transform.localScale = _originalLocalScale;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = _originalLocalPosition;
        
        // Reset material shader properties
        SetPoisonFadeValue(0f);
    }
    
    public void PlayVFX(PortraitVFX vfx)
    {
        ResetVFXState();
        
        switch (vfx)
        {
            case PortraitVFX.None:
                break;
            case PortraitVFX.Hurt:
                TriggerPoisonEffect(_poisonEffectDuration);
                break;
            case PortraitVFX.Surprised:
                TriggerSurpriseEffect();
                break;
            case PortraitVFX.Angry:
                TriggerAngryEffect();
                break;
            case PortraitVFX.VeryAngry:
                TriggerVeryAngryEffect();
                break;
            case PortraitVFX.Puzzled:
                TriggerPuzzledEffect();
                break;
            case PortraitVFX.Annoyed:
                TriggerAnnoyedEffect();
                break;
            default:
                Debug.LogWarning($"VFX {vfx} not implemented for EliPortrait");
                break;
        }
    }

    public Animator GetEyesAnimator() {
        return _eyesAnimator;
    }
}
