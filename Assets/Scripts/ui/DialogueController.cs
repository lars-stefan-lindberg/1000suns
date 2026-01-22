using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Febucci.UI;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public event System.Action OnDialogueClosed;
    public event System.Action OnDialogueClosing;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private float _typeSpeed = 10;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _continueIcon;
    [SerializeField] private GameObject _leftPortrait;
    [SerializeField] private GameObject _rightPortrait;
    [SerializeField] private RectTransform _textBox;
    [SerializeField] private RectTransform _background;
    private Queue<DialogueContent.ParagraphEntry> _paragraphs = new();

    private bool _conversationEnded;
    private string p;
    private bool _isTyping;

    private bool _isDisplayed = false;
    private bool _isFirstParagraph = true;

    void Awake() {
        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
            _continueIcon.SetActive(true);
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
            _continueIcon.SetActive(false);
        });
    }

    public void ShowDialogue(DialogueContent dialogueContent) {
        bool leftMode = dialogueContent.actor == DialogueContent.DialogueActor.Player;
        if(leftMode) {
            _leftPortrait.GetComponent<Image>().sprite = dialogueContent.paragraphEntries[0].portrait;
            _leftPortrait.SetActive(true);
            _rightPortrait.SetActive(false);
            _textBox.offsetMin = new Vector2(0, 0);
            _textBox.offsetMax = new Vector2(0, 0);
        } else {
            _rightPortrait.GetComponent<Image>().sprite = dialogueContent.paragraphEntries[0].portrait;
            _leftPortrait.SetActive(false);
            _rightPortrait.SetActive(true);
            _textBox.offsetMin = new Vector2(-288, 0); //left, bottom
            _textBox.offsetMax = new Vector2(-288, 0); //right, top
        }
        InitializeConversation(dialogueContent);
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
                EndConversation();
                return;
            }
        }

        if(!_isTyping) {
            try {
                DialogueContent.ParagraphEntry paragraphEntry = _paragraphs.Dequeue();
                p = paragraphEntry.text;
                //Make sure the correct sprite image is shown in the dialogue
                if(paragraphEntry.portrait != null) {
                    if(_leftPortrait.activeSelf) {
                        _leftPortrait.GetComponent<Image>().sprite = paragraphEntry.portrait;
                    } else {
                        _rightPortrait.GetComponent<Image>().sprite = paragraphEntry.portrait;
                    }
                }
                if(_isFirstParagraph) {
                    _isFirstParagraph = false;
                } else {
                    SoundFXManager.obj.PlayDialogueConfirm();
                }
                _typeWriter.ShowText(p);
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

    private void InitializeConversation(DialogueContent dialogueContent) {
        for(int i = 0; i < dialogueContent.paragraphEntries.Count; ++i) {
            _paragraphs.Enqueue(dialogueContent.paragraphEntries[i]);
        }
    }

    private void EndConversation() {
        OnDialogueClosing?.Invoke();
        _paragraphs.Clear();
        _conversationEnded = false;
        SoundFXManager.obj.PlayDialogueConfirm();
        _continueIcon.SetActive(false);
        _background.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.5f, RotateMode.FastBeyond360)
              .SetEase(Ease.Linear).OnComplete(() => {
                _isDisplayed = false;
                _typeWriter.ShowText("");
                EventSystem.current.SetSelectedGameObject(null);
                OnDialogueClosed?.Invoke();
              });
    }

    public void HardStopConversation() {
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
