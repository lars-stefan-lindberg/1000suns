using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;
using System;
using System.Collections;

public class IntroController : MonoBehaviour
{
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private DialogueContent _dialogueContent;
    private Queue<string> _paragraphs = new();

    private bool _conversationEnded;
    private string p;
    private bool _isTyping;

    void Start()
    {
        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene()
    {
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        yield return new WaitForSeconds(3f);
        SceneFadeManager.obj.StartFadeIn();
        yield return new WaitForSeconds(7f);
        ShowText();
        yield return new WaitForSeconds(11f);
        SceneFadeManager.obj.StartFadeOut(0.2f);
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        //LevelManager.obj.LoadNextScene();
    }

    void Awake() {
        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
        });
    }

    [ContextMenu("Start cutscene")]
    public void StartCutscene() {
        ShowText();
    }

    public void ShowText() {
        InitializeConversation(_dialogueContent);
        DisplayNextParagraph();
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
                p = _paragraphs.Dequeue();
                _typeWriter.ShowText(p);
            }catch (InvalidOperationException)
            {
                return;
            }
        }

        if(_paragraphs.Count == 0) {
            _conversationEnded = true;
        }
    }

    private void InitializeConversation(DialogueContent dialogueContent) {
        for(int i = 0; i < dialogueContent.paragraphs.Length; ++i) {
            _paragraphs.Enqueue(dialogueContent.paragraphs[i]);
        }
    }

    private void EndConversation() {
        _paragraphs.Clear();
        _conversationEnded = false;
    }

    void OnDestroy() {
        _typeWriter.onTextShowed.RemoveAllListeners();
    }
}
