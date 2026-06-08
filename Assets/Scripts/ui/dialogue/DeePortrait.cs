using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DeePortrait : MonoBehaviour, IPortrait
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
    }
    
    public void PlayVFX(PortraitVFX vfx)
    {
        ResetVFXState();
        
        switch (vfx)
        {
            case PortraitVFX.None:
                break;
            default:
                Debug.LogWarning($"VFX {vfx} not implemented for SootPortrait");
                break;
        }
    }
}
