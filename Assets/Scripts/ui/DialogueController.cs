using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Febucci;
using Febucci.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController obj;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private float _typeSpeed = 10;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _continueIcon;
    private Queue<string> _paragraphs = new();
    private Animator _animator;

    private bool _conversationEnded;
    private string p;
    private Coroutine _typeDialogueCoroutine;
    private bool _isTyping;

    private bool _isDisplayed = false;

    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;

    void Awake() {
        obj = this;
        EventSystem.current.SetSelectedGameObject(_continueButton);
        _animator = GetComponent<Animator>();
        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
            _continueIcon.SetActive(true);
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
            _continueIcon.SetActive(false);
        });
    }

    public async void ShowDialogue(DialogueContent dialogueContent) {
        InitializeConversation(dialogueContent);
        await IsDialogDisplayed();
        DisplayNextParagraph();
    }

    async Task IsDialogDisplayed() {
        while(!_isDisplayed) {
            await Task.Delay(100);
        }
    }

    async Task IsDialogHidden() {
        while(_isDisplayed) {
            await Task.Delay(100);
        }
    }

    public void HideDialogue() {
        EndConversation();
    }

    public void DisplayNextParagraph() {
        if(_paragraphs.Count == 0) {
            if(_conversationEnded && !_isTyping) {
                EndConversation();
                return;
            }
        }

        if(!_isTyping) {
            p = _paragraphs.Dequeue();
            _typeWriter.ShowText(p);
            //_typeDialogueCoroutine = StartCoroutine(TypeDialogueText(p));
        } else {
            FinishParagraphEarly();
        }

        if(_paragraphs.Count == 0) {
            _conversationEnded = true;
        }
    }

    // private IEnumerator TypeDialogueText(string p) {
    //     _isTyping = true;
    //     _continueIcon.SetActive(false);

    //     _dialogueText.text = "";
    //     string originalText = p;
    //     string displayedText;
    //     int alphaIndex = 0;
    //     foreach(char c in p.ToCharArray()) {
    //         alphaIndex++;
    //         _dialogueText.text = originalText;

    //         displayedText = _dialogueText.text.Insert(alphaIndex, HTML_ALPHA);
    //         _dialogueText.text = displayedText;

    //         yield return new WaitForSeconds(MAX_TYPE_TIME / _typeSpeed);
    //     }
    //     _isTyping = false;
    //     _continueIcon.SetActive(true);
    // }

    private void InitializeConversation(DialogueContent dialogueContent) {
        if(!gameObject.activeSelf) {
            gameObject.SetActive(true);
        }

        for(int i = 0; i < dialogueContent.paragraphs.Length; ++i) {
            _paragraphs.Enqueue(dialogueContent.paragraphs[i]);
        }
    }

    private async void EndConversation() {
        _paragraphs.Clear();
        _conversationEnded = false;
        _animator.SetTrigger("hide");
        await IsDialogHidden();

        if(gameObject.activeSelf) {
            gameObject.SetActive(false);
        }
        PlayerMovement.obj.UnFreeze();
    }

    private void FinishParagraphEarly() {
        //StopCoroutine(_typeDialogueCoroutine);
        //_dialogueText.text = p;
        _typeWriter.SkipTypewriter();
        _isTyping = false;
        _continueIcon.SetActive(true);
    }

    public void SetDialogDisplayed() {
        _isDisplayed = true;
    }

    public void SetDialogHidden() {
        _isDisplayed = false;
    }

    public bool IsDisplayed() {
        return _isDisplayed;
    }

    public void FocusDialogue() {
        EventSystem.current.SetSelectedGameObject(_continueButton);
    }

    void OnDestroy() {
        _typeWriter.onTextShowed.RemoveAllListeners();
        obj = null;
    }
}
