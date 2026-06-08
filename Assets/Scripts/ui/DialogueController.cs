using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Febucci.UI;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public class DialogueController : MonoBehaviour
{
    public event System.Action OnDialogueClosed;
    public event System.Action OnDialogueClosing;
    [SerializeField] private GameObject _portraitContainer;
    [SerializeField] private GameObject _textBox;
    [SerializeField] private LayoutElement _leftSpacer;
    [SerializeField] private LayoutElement _rightSpacer;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private float _typeSpeed = 10;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private GameObject _eliPortrait;
    [SerializeField] private GameObject _sootPortrait;
    [SerializeField] private GameObject _deePortrait;
    [SerializeField] private GameObject _audioAnchorPrefab;

    [SerializeField] private RectTransform _background;
    
    [Header("Blinking Settings")]
    [SerializeField] private float _blinkMinDelay = 2f;
    [SerializeField] private float _blinkMaxDelay = 6f;
    [SerializeField] [Range(0f, 1f)] private float _doubleBlinkChance = 0.15f;
    private string _tableCollectionName = "Dialogue_Text";

    private Queue<DialogueContent.ParagraphEntry> _paragraphs = new();
    private DialogueAudio _dialogueAudio;
    private DialogueSoundEffectPlayer _currentSoundEffectPlayer;

    private bool _conversationEnded;
    private string p;
    private bool _isTyping;

    private bool _isDisplayed = false;
    private bool _isFirstParagraph = true;
    private bool _isLastDialogue = false;
    private Coroutine _loadAndPlayCoroutine;
    private readonly int SPACER_MARGIN_SMALL = 56;
    private readonly int SPACER_MARGIN_BIG = 96;
    private GameObject _currentPortrait;
    private IPortrait _portraitInterface;
    private GameObject _audioAnchorInstance;
    private DialogueAudioAnchor _audioAnchor;
    private bool _isDialogueLeft;
    private Coroutine _blinkCoroutine;

    void Awake() {
        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
            _continueIcon.SetActive(true);
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
            _continueIcon.SetActive(false);
        });
        _dialogueAudio = GetComponent<DialogueAudio>();
    }

    //In longer conversations we only want to play open sfx at the start of the conversation. Use playOpenSfx to control this.
    public void ShowDialogue(DialogueContent dialogueContent, bool isFirstDialogue = false, bool isLastDialogue = false) {
        if(_audioAnchorInstance == null && _audioAnchorPrefab != null) {
            _audioAnchorInstance = Instantiate(_audioAnchorPrefab);
            _audioAnchor = _audioAnchorInstance.GetComponent<DialogueAudioAnchor>();
        }
        
        // Hide current portrait if one is active
        if(_currentPortrait != null && _currentPortrait.activeSelf) {
            _currentPortrait.SetActive(false);
        }
        
        // Try to find an existing inactive portrait for this actor
        _currentPortrait = FindInactivePortraitForActor(dialogueContent.actor);
        
        if(_currentPortrait == null) {
            // No existing portrait found, instantiate a new one
            if(dialogueContent.actor == DialogueContent.DialogueActor.Player) {
                _currentPortrait = Instantiate(_eliPortrait);
                _currentPortrait.transform.SetParent(_portraitContainer.transform);
                _currentPortrait.transform.localPosition = new Vector3(-5.875f, -45.625f, 0);
                _currentPortrait.transform.localScale = _portraitContainer.transform.localScale;
                _currentPortrait.transform.localRotation = _portraitContainer.transform.localRotation;
                _portraitInterface = _currentPortrait.GetComponent<EliPortrait>();
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<EliDialogueSoundEffectPlayer>();
            } else if(dialogueContent.actor == DialogueContent.DialogueActor.CaveAvatar) {
                _currentPortrait = Instantiate(_sootPortrait);
                _currentPortrait.transform.SetParent(_portraitContainer.transform);
                _currentPortrait.transform.localPosition = new Vector3(0, -12f, 0);
                _currentPortrait.transform.localScale = _portraitContainer.transform.localScale;
                _currentPortrait.transform.localRotation = _portraitContainer.transform.localRotation;
                _portraitInterface = _currentPortrait.GetComponent<SootPortrait>();
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<SootDialogueSoundEffectPlayer>();
            } else if(dialogueContent.actor == DialogueContent.DialogueActor.Dee) {
                _currentPortrait = Instantiate(_deePortrait);
                _currentPortrait.transform.SetParent(_portraitContainer.transform);
                _currentPortrait.transform.localPosition = new Vector3(0, -56.0625f, 0);
                _currentPortrait.transform.localScale = _portraitContainer.transform.localScale;
                _currentPortrait.transform.localRotation = _portraitContainer.transform.localRotation;
                _portraitInterface = _currentPortrait.GetComponent<DeePortrait>();
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<DeeDialogueSoundEffectPlayer>();
            }
        } else {
            // Reusing existing portrait - get components
            _portraitInterface = _currentPortrait.GetComponent<IPortrait>();
            if(dialogueContent.actor == DialogueContent.DialogueActor.Player) {
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<EliDialogueSoundEffectPlayer>();
            } else if(dialogueContent.actor == DialogueContent.DialogueActor.CaveAvatar) {
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<SootDialogueSoundEffectPlayer>();
            } else if(dialogueContent.actor == DialogueContent.DialogueActor.Dee) {
                _currentSoundEffectPlayer = _currentPortrait.GetComponent<DeeDialogueSoundEffectPlayer>();
            }
        }
        
        // Activate the portrait
        _currentPortrait.SetActive(true);
        
        DialogueContent.Emotion firstEmotion = dialogueContent.paragraphEntries[0].emotion;
        _portraitInterface.SwitchEmotion(firstEmotion.ToString());
        
        _isDialogueLeft = dialogueContent.IsLeft;
        _portraitInterface.IsLeft = dialogueContent.IsLeft;
        
        if(dialogueContent.IsLeft) {
            _portraitContainer.transform.SetSiblingIndex(1);
            _rightSpacer.preferredWidth = SPACER_MARGIN_SMALL;
            _leftSpacer.preferredWidth = SPACER_MARGIN_BIG;
        } else {
            _portraitContainer.transform.SetSiblingIndex(2);
            _rightSpacer.preferredWidth = SPACER_MARGIN_BIG;
            _leftSpacer.preferredWidth = SPACER_MARGIN_SMALL;
        }

        // Optionally flip the portrait texture horizontally
        _currentPortrait.transform.localScale = new Vector3(dialogueContent.IsFlipped ? -1 : 1, 1, 1);
        
        // Capture the scale after flipping so VFX reset uses the correct scale
        if (_portraitInterface is SootPortrait sootPortrait)
        {
            sootPortrait.CaptureOriginalScale();
        }
        else if (_portraitInterface is EliPortrait eliPortrait)
        {
            eliPortrait.CaptureOriginalScale();
        }
        else if (_portraitInterface is DeePortrait deePortrait)
        {
            deePortrait.CaptureOriginalScale();
        }
        
        InitializeConversation(dialogueContent);

        if(isFirstDialogue)
            _dialogueAudio.PlayOpen();
        _isLastDialogue = isLastDialogue;

        _background.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.5f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear).OnComplete(() => {
                    _isDisplayed = true;
                    _isFirstParagraph = true;
                    DisplayNextParagraph();
                    EventSystem.current.SetSelectedGameObject(_continueButton);
                    StartBlinking();
                });
    }

    public void DisplayNextParagraph() {
        if(_paragraphs.Count == 0) {
            if(_conversationEnded && !_isTyping) {
                EndDialogue();
                return;
            }
        }

        if(!_isTyping) {
            try {
                DialogueContent.ParagraphEntry paragraphEntry = _paragraphs.Dequeue();
                p = paragraphEntry.text;
                
                ResetToIdleState();
                StartBlinking();
                
                DialogueContent.Emotion emotion = paragraphEntry.emotion;
                _portraitInterface.SwitchEmotion(emotion.ToString());
                
                _portraitInterface.PlayVFX(paragraphEntry.vfx);
                
                // Set hover speed for SootPortrait
                if (_portraitInterface is SootPortrait sootPortrait)
                {
                    sootPortrait.SetHoverSpeed(paragraphEntry.hoverSpeed);
                }
                
                if(_currentSoundEffectPlayer != null && _audioAnchor != null) {
                    GameObject audioAnchor = _isDialogueLeft ? _audioAnchor.GetLeftAnchor() : _audioAnchor.GetRightAnchor();
                    _currentSoundEffectPlayer.Play(paragraphEntry.soundEffect, audioAnchor);
                }
                
                if(_isFirstParagraph) {
                    _isFirstParagraph = false;
                } else {
                    if(paragraphEntry.soundEffect == DialogueSoundEffect.None) {
                        _dialogueAudio.PlayConfirm();
                    }
                }
                _loadAndPlayCoroutine = StartCoroutine(LoadAndPlay(p));
            }catch (InvalidOperationException)
            {
                return;
            }
        } else {
            FinishParagraphEarly();
        }

        if(_paragraphs.Count == 0) {
            _conversationEnded = true;
        }
    }

    private IEnumerator LoadAndPlay(string key)
    {
        yield return LocalizationSettings.InitializationOperation;

        var tableHandle = LocalizationSettings.StringDatabase
            .GetTableAsync(_tableCollectionName);

        yield return tableHandle;

        if (!tableHandle.IsDone || tableHandle.Result == null)
        {
            Debug.LogError($"Localization table not found: {_tableCollectionName}");
            yield break;
        }

        StringTable table = tableHandle.Result;

        var entry = table.GetEntry(key);

        if (entry == null)
        {
            //Debug.LogError($"Localization key not found: {key}");
            _typeWriter.ShowText(key);
            yield break;
        }

        string translatedText = entry.GetLocalizedString();

        _typeWriter.ShowText(translatedText);
    }

    private void InitializeConversation(DialogueContent dialogueContent) {
        for(int i = 0; i < dialogueContent.paragraphEntries.Count; ++i) {
            _paragraphs.Enqueue(dialogueContent.paragraphEntries[i]);
        }
    }

    private void EndDialogue() {
        OnDialogueClosing?.Invoke();
        _paragraphs.Clear();
        _conversationEnded = false;
        _dialogueAudio.PlayConfirm();
        if(_isLastDialogue) {
            _dialogueAudio.PlayClose();
        }
        _continueIcon.SetActive(false);
        StopBlinking();
        
        _background.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.5f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear).OnComplete(() => {
                _isDisplayed = false;
                _typeWriter.ShowText("");
                EventSystem.current.SetSelectedGameObject(null);
                // Hide portrait but keep it for reuse
                if(_currentPortrait != null) {
                    _currentPortrait.SetActive(false);
                }
                OnDialogueClosed?.Invoke();
              });
        _loadAndPlayCoroutine = null;
    }

    public void HardStopConversation() {
        if(_loadAndPlayCoroutine != null) {
            StopCoroutine(_loadAndPlayCoroutine);
        }
        _paragraphs.Clear();
        _conversationEnded = false;
        _continueIcon.SetActive(false);
        StopBlinking();
        _background.localRotation = Quaternion.Euler(90f, 0f, 0f);
        _isDisplayed = false;
        _typeWriter.ShowText("");
        EventSystem.current.SetSelectedGameObject(null);
        
        // Hide portrait
        if(_currentPortrait != null) {
            _currentPortrait.SetActive(false);
        }
    }
    
    /// <summary>
    /// Finds an inactive portrait in the container for the specified actor.
    /// </summary>
    private GameObject FindInactivePortraitForActor(DialogueContent.DialogueActor actor) {
        if(_portraitContainer == null) return null;
        
        // Search through children of portrait container for inactive portraits
        for(int i = 0; i < _portraitContainer.transform.childCount; i++) {
            GameObject child = _portraitContainer.transform.GetChild(i).gameObject;
            if(!child.activeSelf) {
                // Check if this portrait matches the actor type
                if(actor == DialogueContent.DialogueActor.Player && child.GetComponent<EliPortrait>() != null) {
                    return child;
                } else if(actor == DialogueContent.DialogueActor.CaveAvatar && child.GetComponent<SootPortrait>() != null) {
                    return child;
                } else if(actor == DialogueContent.DialogueActor.Dee && child.GetComponent<DeePortrait>() != null) {
                    return child;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Call this method when the entire conversation has ended to clean up all resources.
    /// This destroys portraits and audio anchors that were kept for reuse.
    /// </summary>
    public void CleanUp() {
        // Stop any running coroutines
        if(_loadAndPlayCoroutine != null) {
            StopCoroutine(_loadAndPlayCoroutine);
            _loadAndPlayCoroutine = null;
        }
        StopBlinking();
        
        // Clear state
        _paragraphs.Clear();
        _conversationEnded = false;
        _isDisplayed = false;
        _isFirstParagraph = true;
        _isLastDialogue = false;
        _isTyping = false;
        
        // Clean up UI
        _continueIcon.SetActive(false);
        _typeWriter.ShowText("");
        EventSystem.current.SetSelectedGameObject(null);
        
        // Reset background rotation
        _background.localRotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Destroy all portraits in the container
        if(_portraitContainer != null) {
            for(int i = _portraitContainer.transform.childCount - 1; i >= 0; i--) {
                Destroy(_portraitContainer.transform.GetChild(i).gameObject);
            }
        }
        
        _currentPortrait = null;
        _portraitInterface = null;
        _currentSoundEffectPlayer = null;
        
        // Destroy audio anchor
        if(_audioAnchorInstance != null) {
            Destroy(_audioAnchorInstance);
            _audioAnchorInstance = null;
            _audioAnchor = null;
        }
        
    }

    private void FinishParagraphEarly() {
        _typeWriter.SkipTypewriter();
        _isTyping = false;
        _continueIcon.SetActive(true);
    }

    public bool IsDisplayed() {
        return _isDisplayed;
    }

    public void FocusDialogue() {
        EventSystem.current.SetSelectedGameObject(_continueButton);
    }

    void OnDestroy() {
        _typeWriter.onTextShowed.RemoveAllListeners();
        if(_audioAnchorInstance != null) {
            Destroy(_audioAnchorInstance);
        }
    }
    
    private IEnumerator BlinkRoutine() {
        while(true) {
            float delay = UnityEngine.Random.Range(_blinkMinDelay, _blinkMaxDelay);
            yield return new WaitForSeconds(delay);
            
            if(_portraitInterface != null) {
                float roll = UnityEngine.Random.value;
                if(roll < _doubleBlinkChance) {
                    _portraitInterface.DoubleBlink();
                } else {
                    _portraitInterface.Blink();
                }
            }
        }
    }
    
    private void StartBlinking() {
        StopBlinking();
        if(_portraitInterface != null) {
            _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }
    
    private void StopBlinking() {
        if(_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }
    
    private void ResetToIdleState() {
        if(_portraitInterface != null) {
            Animator animator = _portraitInterface.GetEyesAnimator();
            if(animator != null) {
                animator.Play("idle", -1, 0f);
            }
        }
    }
}
