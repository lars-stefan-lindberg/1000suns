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

    [SerializeField] private RectTransform _background;
    private string _tableCollectionName = "Dialogue_Text";

    private Queue<DialogueContent.ParagraphEntry> _paragraphs = new();
    private DialogueAudio _dialogueAudio;

    private bool _conversationEnded;
    private string p;
    private bool _isTyping;

    private bool _isDisplayed = false;
    private bool _isFirstParagraph = true;
    private bool _isLastDialogue = false;
    private Coroutine _loadAndPlayCoroutine;
    private readonly int SPACER_MARGIN_SMALL = 56;
    private readonly int SPACER_MARGIN_BIG = 96;
    private GameObject _portrait;
    private IPortrait _portraitInterface;

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
        if(dialogueContent.actor == DialogueContent.DialogueActor.Player) {
            _portrait = Instantiate(_eliPortrait);
            _portrait.transform.SetParent(_portraitContainer.transform);
            _portrait.transform.localPosition = new Vector3(-5.875f, -45.625f, 0);
            _portrait.transform.localScale = _portraitContainer.transform.localScale;
            _portrait.transform.localRotation = _portraitContainer.transform.localRotation;
            _portraitInterface = _portrait.GetComponent<EliPortrait>();
        }
        
        DialogueContent.Emotion firstEmotion = dialogueContent.paragraphEntries[0].emotion;
        _portraitInterface.SwitchEmotion(firstEmotion.ToString());
        
        
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
        _portrait.transform.localScale = new Vector3(dialogueContent.IsFlipped ? -1 : 1, 1, 1);
        
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
                
                DialogueContent.Emotion emotion = paragraphEntry.emotion;
                _portraitInterface.SwitchEmotion(emotion.ToString());
                
                if(_isFirstParagraph) {
                    _isFirstParagraph = false;
                } else {
                    _dialogueAudio.PlayConfirm();
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
        _background.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.5f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear).OnComplete(() => {
                _isDisplayed = false;
                _typeWriter.ShowText("");
                EventSystem.current.SetSelectedGameObject(null);
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
        _background.localRotation = Quaternion.Euler(90f, 0f, 0f);
        _isDisplayed = false;
        _typeWriter.ShowText("");
        EventSystem.current.SetSelectedGameObject(null);
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
    }
}
