using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;

[System.Serializable]
public class TutorialStep
{
    public LocalizedString localizedString;
    public List<InputActionReference> inputActions = new List<InputActionReference>();
    public List<InputIconManager.Direction> inputDirections = new List<InputIconManager.Direction>();
    public List<InputIconManager.StickType> inputStickTypes = new List<InputIconManager.StickType>();
}

public delegate bool TutorialCondition();

[System.Serializable]
public class ConditionalTutorialStep
{
    public TutorialStep step;
    [System.NonSerialized] public TutorialCondition condition;
}

public class TutorialStrip : MonoBehaviour
{
    [SerializeField] private Canvas _tutorialStripCanvas;
    [SerializeField] private CanvasGroup _tutorialStripCanvasGroup;
    [SerializeField] private TMP_Text _tutorialTMPText;
    
    // Legacy single-step fields (for backward compatibility)
    [SerializeField] private LocalizedString _tutorialLocalizedString;
    [SerializeField] private List<InputActionReference> _inputActions = new List<InputActionReference>();
    [SerializeField] private List<InputIconManager.Direction> _inputDirections = new List<InputIconManager.Direction>();
    [SerializeField] private List<InputIconManager.StickType> _inputStickTypes = new List<InputIconManager.StickType>();
    
    // Multi-step conditional system
    private List<ConditionalTutorialStep> _conditionalSteps = new List<ConditionalTutorialStep>();
    private bool _isConditionalMode = false;
    
    // State tracking
    private int _currentStepIndex = 0;
    private int _targetStepIndex = 0;
    private TransitionState _transitionState = TransitionState.Idle;
    private float _lastConditionChangeTime = -100f;
    private bool _isShown = false;
    
    // Fade durations
    private readonly float _fadeInDuration = 1f;
    private readonly float _fadeOutDuration = 1f;
    private readonly float _quickFadeOutDuration = 0.3f;
    private readonly float _textTransitionFadeOutDuration = 0.1f;
    private readonly float _textTransitionFadeInDuration = 0.1f;
    private readonly float _conditionDebounceTime = 0.1f;
    
    private Tween _currentFadeTween;
    private Tween _currentTextFadeTween;
    
    private enum TransitionState
    {
        Idle,
        FadingOut,
        FadingIn
    }

    void Awake() {
        _tutorialStripCanvas.worldCamera = Camera.main;
        _tutorialStripCanvas.sortingLayerName = "UI";

        // Initialize with legacy single-step if not using conditional mode
        if (!_isConditionalMode && _tutorialLocalizedString != null)
        {
            UpdateInputStringWithIcons();
        }
    }
    
    void OnEnable() {
        // Reset state when GameObject is enabled
        // This ensures clean state whether it's first activation or re-activation
        if (!_isShown)
        {
            _tutorialStripCanvasGroup.alpha = 0f;
            _tutorialStripCanvasGroup.interactable = false;
            _tutorialStripCanvasGroup.blocksRaycasts = false;
        }
        
        // Ensure text starts fully opaque (transitions will fade the text, not the canvas group)
        _tutorialTMPText.alpha = 1f;
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        if (_isConditionalMode && _transitionState == TransitionState.Idle)
        {
            EvaluateConditions();
        }
    }
    
    // Public API for initializing conditional multi-step tutorials
    public void InitializeConditional(List<ConditionalTutorialStep> steps)
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogError("TutorialStrip: Cannot initialize with empty steps list");
            return;
        }
        
        _conditionalSteps = steps;
        _isConditionalMode = true;
        _currentStepIndex = 0;
        _targetStepIndex = 0;
        
        // Initialize the first step
        UpdateCurrentStepContent();
    }
    
    // Helper to set condition for a specific step
    public void SetStepCondition(int stepIndex, TutorialCondition condition)
    {
        if (stepIndex >= 0 && stepIndex < _conditionalSteps.Count)
        {
            _conditionalSteps[stepIndex].condition = condition;
        }
    }

    private void EvaluateConditions()
    {
        if (_conditionalSteps.Count == 0) return;
        
        // Find the highest priority step whose condition is true
        // Steps are checked in reverse order (later steps have higher priority)
        int newTargetIndex = 0; // Default to first step if no conditions are met
        
        for (int i = _conditionalSteps.Count - 1; i >= 0; i--)
        {
            var conditionalStep = _conditionalSteps[i];
            
            // If no condition is set, or condition returns true, use this step
            if (conditionalStep.condition == null || conditionalStep.condition())
            {
                newTargetIndex = i;
                break;
            }
        }
        
        // If target changed, check debounce and initiate transition
        if (newTargetIndex != _currentStepIndex)
        {
            float timeSinceLastChange = Time.time - _lastConditionChangeTime;
            
            if (timeSinceLastChange >= _conditionDebounceTime)
            {
                _targetStepIndex = newTargetIndex;
                _lastConditionChangeTime = Time.time;
                StartTransitionToStep(_targetStepIndex);
            }
        }
    }
    
    private void StartTransitionToStep(int stepIndex)
    {
        if (_transitionState != TransitionState.Idle)
        {
            // Already transitioning - update target and let current transition complete
            _targetStepIndex = stepIndex;
            return;
        }
        
        _transitionState = TransitionState.FadingOut;
        
        // Kill any existing text tween
        if (_currentTextFadeTween != null && _currentTextFadeTween.IsActive())
        {
            _currentTextFadeTween.Kill();
        }
        
        // Fade out only the text (not the entire tutorial strip)
        _currentTextFadeTween = _tutorialTMPText
            .DOFade(0f, _textTransitionFadeOutDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(OnTransitionFadeOutComplete);
    }
    
    private void OnTransitionFadeOutComplete()
    {
        // Check if target changed during fade out
        if (_targetStepIndex != _currentStepIndex)
        {
            _currentStepIndex = _targetStepIndex;
            UpdateCurrentStepContent();
            
            _transitionState = TransitionState.FadingIn;
            
            // Fade in the new text
            _currentTextFadeTween = _tutorialTMPText
                .DOFade(1f, _textTransitionFadeInDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnComplete(OnTransitionFadeInComplete);
        }
        else
        {
            // Target is same as current, just fade text back in
            _transitionState = TransitionState.Idle;
            _tutorialTMPText.alpha = 1f;
        }
    }
    
    private void OnTransitionFadeInComplete()
    {
        _transitionState = TransitionState.Idle;
        
        // Check if condition changed again during fade in
        if (_isConditionalMode)
        {
            EvaluateConditions();
        }
    }
    
    private void UpdateCurrentStepContent()
    {
        if (!_isConditionalMode || _currentStepIndex >= _conditionalSteps.Count)
            return;
        
        TutorialStep currentStep = _conditionalSteps[_currentStepIndex].step;
        
        if (currentStep == null || currentStep.localizedString == null)
        {
            Debug.LogWarning("TutorialStrip: Current step or localized string is null");
            return;
        }
        
        // Unsubscribe from all localized strings first
        foreach (var conditionalStep in _conditionalSteps)
        {
            if (conditionalStep.step?.localizedString != null)
            {
                conditionalStep.step.localizedString.StringChanged -= UpdateInputText;
            }
        }
        
        // Update with new step's content (this will subscribe to the current step)
        UpdateInputStringWithIconsForStep(currentStep);
    }

    public void Show() {
        // If already shown, do nothing
        if (_isShown)
        {
            return;
        }
        
        // If currently fading in, do nothing
        if (_currentFadeTween != null && _currentFadeTween.IsActive())
        {
            // Check if we're fading in (target is 1) vs fading out (target is 0)
            // If fading in, let it continue
            if (_tutorialStripCanvasGroup.alpha < 1f)
            {
                return;
            }
        }

        // Kill any existing tween (e.g., if we were fading out)
        if (_currentFadeTween != null && _currentFadeTween.IsActive())
        {
            _currentFadeTween.Kill();
        }
        
        // Force set to 0 immediately and disable interaction
        _tutorialStripCanvasGroup.alpha = 0f;
        _tutorialStripCanvasGroup.interactable = false;
        _tutorialStripCanvasGroup.blocksRaycasts = false;

        // Use From() to explicitly set the starting value, ensuring DOTween starts from 0
        _currentFadeTween = _tutorialStripCanvasGroup
            .DOFade(1f, _fadeInDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _tutorialStripCanvasGroup.interactable = true;
                _tutorialStripCanvasGroup.blocksRaycasts = true;
                _isShown = true;
            });
    }

    public void Hide() {
        HideInternal(_fadeOutDuration);
    }
    
    public void HideQuick() {
        HideInternal(_quickFadeOutDuration);
    }
    
    private void HideInternal(float fadeDuration) {
        // If already hidden or hiding, do nothing
        if (!_isShown && (_currentFadeTween == null || !_currentFadeTween.IsActive()))
        {
            return;
        }
        
        _tutorialStripCanvasGroup.interactable = false;
        _tutorialStripCanvasGroup.blocksRaycasts = false;

        // Kill any existing tween
        if (_currentFadeTween != null && _currentFadeTween.IsActive())
        {
            _currentFadeTween.Kill();
        }

        _currentFadeTween = _tutorialStripCanvasGroup
            .DOFade(0f, fadeDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _isShown = false;
            });
    }

    // Legacy method for backward compatibility
    private void UpdateInputStringWithIcons() {
        if (_tutorialLocalizedString == null) return;
        
        string deviceLayoutName = InputDeviceListener.obj.GetCurrentDeviceLayoutName();
        
        TMP_SpriteAsset spriteAsset = InputIconManager.obj.GetSpriteAsset(deviceLayoutName);
        _tutorialTMPText.spriteAsset = spriteAsset;
        
        var spriteArguments = new List<object>();
        
        for (int i = 0; i < _inputActions.Count; i++)
        {
            var actionReference = _inputActions[i];
            if (actionReference == null || actionReference.action == null)
            {
                Debug.LogWarning("TutorialStrip: InputActionReference is null or has no action");
                continue;
            }
            
            // Get direction if available, otherwise use None
            InputIconManager.Direction direction = i < _inputDirections.Count ? _inputDirections[i] : InputIconManager.Direction.None;
            
            // Get stick type if available, otherwise use None
            InputIconManager.StickType stickType = i < _inputStickTypes.Count ? _inputStickTypes[i] : InputIconManager.StickType.None;
            
            string spriteName = InputIconManager.obj.GetSpriteNameForAction(actionReference, direction, stickType);
            spriteArguments.Add($"<size=150%><voffset=-1px><sprite name=\"{spriteName}\" tint=1></voffset></size>");
        }
        
        _tutorialLocalizedString.Arguments = spriteArguments.ToArray();
        _tutorialLocalizedString.StringChanged += UpdateInputText;
        _tutorialLocalizedString.RefreshString();
    }
    
    // New method for conditional steps
    private void UpdateInputStringWithIconsForStep(TutorialStep step) {
        if (step == null || step.localizedString == null) return;
        
        string deviceLayoutName = InputDeviceListener.obj.GetCurrentDeviceLayoutName();
        
        TMP_SpriteAsset spriteAsset = InputIconManager.obj.GetSpriteAsset(deviceLayoutName);
        _tutorialTMPText.spriteAsset = spriteAsset;
        
        var spriteArguments = new List<object>();
        
        for (int i = 0; i < step.inputActions.Count; i++)
        {
            var actionReference = step.inputActions[i];
            if (actionReference == null || actionReference.action == null)
            {
                Debug.LogWarning("TutorialStrip: InputActionReference is null or has no action");
                continue;
            }
            
            // Get direction if available, otherwise use None
            InputIconManager.Direction direction = i < step.inputDirections.Count ? step.inputDirections[i] : InputIconManager.Direction.None;
            
            // Get stick type if available, otherwise use None
            InputIconManager.StickType stickType = i < step.inputStickTypes.Count ? step.inputStickTypes[i] : InputIconManager.StickType.None;
            
            string spriteName = InputIconManager.obj.GetSpriteNameForAction(actionReference, direction, stickType);
            spriteArguments.Add($"<size=150%><voffset=-1px><sprite name=\"{spriteName}\" tint=1></voffset></size>");
        }
        
        step.localizedString.Arguments = spriteArguments.ToArray();
        step.localizedString.StringChanged += UpdateInputText;
        step.localizedString.RefreshString();
    }

    private void UpdateInputText(string value) {
        _tutorialTMPText.text = value;
    }

    void OnDestroy() {
        // Kill any active tweens
        if (_currentFadeTween != null && _currentFadeTween.IsActive())
        {
            _currentFadeTween.Kill();
        }
        
        if (_currentTextFadeTween != null && _currentTextFadeTween.IsActive())
        {
            _currentTextFadeTween.Kill();
        }
        
        // Unsubscribe from legacy localized string
        if (_tutorialLocalizedString != null)
        {
            _tutorialLocalizedString.StringChanged -= UpdateInputText;
        }
        
        // Unsubscribe from all conditional step localized strings
        if (_isConditionalMode)
        {
            foreach (var conditionalStep in _conditionalSteps)
            {
                if (conditionalStep.step?.localizedString != null)
                {
                    conditionalStep.step.localizedString.StringChanged -= UpdateInputText;
                }
            }
        }
    }
}
