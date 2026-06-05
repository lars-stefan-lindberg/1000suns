using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EliPortrait : MonoBehaviour, IPortrait
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

    void Awake()
    {
        _emotionCache = new Dictionary<DialogueContent.Emotion, EmotionData>();
        foreach (var emotionData in _emotionMappings)
        {
            _emotionCache[emotionData.emotion] = emotionData;
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

    [ContextMenu("SwitchEmotion")]
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
}
