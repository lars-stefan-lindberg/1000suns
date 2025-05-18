using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class IntroController : MonoBehaviour
{
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private SceneField _caveRoom1;
    [SerializeField] private SceneField _introScene;
    [SerializeField] private GameObject[] _gameObjectsToDisable;
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
        yield return new WaitForSeconds(0.4f);
        MusicManager.obj.PlayIntroSong();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        yield return new WaitForSeconds(3f);
        SceneFadeManager.obj.StartFadeIn();
        yield return new WaitForSeconds(7f);
        StartText();
        yield return new WaitForSeconds(7f);
        DisplayNextParagraph();
        yield return new WaitForSeconds(13f);
        SceneFadeManager.obj.StartFadeOut(0.2f);
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        
        foreach(GameObject gameObject in _gameObjectsToDisable) {
            gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2f);
        MusicManager.obj.StopPlaying();

        AsyncOperation loadCaveOperation = SceneManager.LoadSceneAsync(_caveRoom1, LoadSceneMode.Additive);
        while(!loadCaveOperation.isDone) {
            yield return null;
        }
        Scene caveRoom1 = SceneManager.GetSceneByName(_caveRoom1.SceneName);
        SceneManager.SetActiveScene(caveRoom1);

        GameObject[] sceneGameObjects = caveRoom1.GetRootGameObjects();
        GameObject playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
        Collider2D _playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();
        
        CaveAvatar.obj.gameObject.SetActive(false);
        
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        Player.obj.transform.position = _playerSpawningCollider.transform.position;
        PlayerMovement.obj.DisablePlayerMovement();
        Player.obj.SetHasCape(false);

        GameObject levelSwitcherGameObject = sceneGameObjects.First(gameObject => gameObject.CompareTag("LevelSwitcher"));
        LevelSwitcher levelSwitcher = levelSwitcherGameObject.GetComponent<LevelSwitcher>();
        levelSwitcher.LoadNextRoom();

        SceneManager.UnloadSceneAsync(_introScene.SceneName);
    }

    void Awake() {
        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
        });
    }

    public void StartText() {
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
