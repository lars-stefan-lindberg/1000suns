using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Febucci.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using DG.Tweening;
using FMODUnity;

public class Cave23DDeeRoomManager : MonoBehaviour
{
    [SerializeField] private MusicTrack _music;
    [SerializeField] private GameEventId _dreamRoomCompleted;
    [SerializeField] private float _startMusicDuration = 2f;
    [SerializeField] private Canvas _textCanvas;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private float _textFadeDuration = 2f;
    [SerializeField] private float _textFadeOutDelay = 5f;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private GameObject _cameraFollowObject;
    [SerializeField] private GameObject _blockingWallLeft;
    [SerializeField] private Transform _background;
    [SerializeField] private PrisonerCutScene _prisoner1;
    [SerializeField] private PrisonerCutScene _prisoner2;
    [Header("Ummara")]
    [SerializeField] private ParticleSystem _ummaraParticles;
    [SerializeField] private UmmaraEyesManager _ummaraEyes;
    [SerializeField] private float _ummaraEyesTransparency = 0.91f;
    [SerializeField] private UmmaraBodyManager _ummaraBody;
    [SerializeField] private float _ummaraBodyTransparency = 0.34f;
    [SerializeField] private float _ummaraBodyLightOverlayAlpha = 0.411f;
    [SerializeField] private float _ummaraFadeInDuration = 4f;
    [SerializeField] private float _ummaraFadeOutDuration = 2f;
    [SerializeField] private GameObject _ummara;
    [SerializeField] private SceneField _teleportBackToScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private EventReference _teleport;

    private Queue<DialogueContent.ParagraphEntry> _paragraphs = new();
    private bool _conversationEnded;
    private string p;
    private bool _isTyping;
    private TextMeshProUGUI _tmpText;
    private Coroutine _transitionIntoRoomCoroutine;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _fadeOutTextCoroutine;

    void Awake() {
        _textCanvas.worldCamera = Camera.main;
        _textCanvas.sortingLayerName = "UI";

        _tmpText = _textCanvas.GetComponentInChildren<TextMeshProUGUI>();

        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
            StartFadeOutTextAfterDelay();
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
        });
    }

    void Start()
    {
        //Set player position and state
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(false);
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinPlayer.obj.transform.position = _spawnPoint.transform.position;
        ShadowTwinMovement.obj.Freeze();
        ShadowTwinMovement.obj.SetNewPower();

        PlayerPowersManager.obj.DeeCanForcePull = false;        
        DustParticleMgr.obj.Enabled = false;

        _transitionIntoRoomCoroutine = StartCoroutine(TransitionIntoRoom());
    }

    private IEnumerator TransitionIntoRoom() {
        //Set camera
        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, _cameraFollowObject.transform, _spawnPoint.transform.position);
        yield return new WaitForSeconds(1f);
        SceneManager.SetActiveScene(gameObject.scene);


        //All loading should be completed. Start fading in room
        yield return new WaitForSeconds(1.5f);
        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        _ummara.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(StartMusic());
        GameManager.obj.IsPauseAllowed = true;
        PauseMenuManager.obj.SetPauseMusicInsteadOfMuffle(true);

        _cutsceneCoroutine = StartCoroutine(Cutscene());
    }

    private IEnumerator StartMusic() {
        yield return new WaitForSeconds(_startMusicDuration);
        MusicManager.obj.Play(_music);
    }

    void Update()
    {
        if(ShadowTwinPlayer.obj.transform.position.x > _cameraFollowObject.transform.position.x) {
            _cameraFollowObject.transform.position = new Vector3(ShadowTwinPlayer.obj.transform.position.x, _cameraFollowObject.transform.position.y, _cameraFollowObject.transform.position.z);
        }
        _ummara.transform.position = new Vector2(_cameraFollowObject.transform.position.x, _ummara.transform.position.y);
        _blockingWallLeft.transform.position = new Vector3(_cameraFollowObject.transform.position.x - 20.5f, _blockingWallLeft.transform.position.y, _blockingWallLeft.transform.position.z);
        _background.position = new Vector3(_cameraFollowObject.transform.position.x, _background.position.y, _background.position.z);
    }
    
    private IEnumerator Cutscene()
    {
        yield return new WaitForSeconds(3f);
        //Fade in Um'mara
        StartCoroutine(_ummaraBody.FadeInSpriteTo(_ummaraBodyTransparency, _ummaraFadeInDuration));
        StartCoroutine(_ummaraEyes.FadeInSpritesTo(_ummaraEyesTransparency, _ummaraFadeInDuration));
        StartCoroutine(_ummaraBody.FadeInLightTo(_ummaraBodyLightOverlayAlpha, _ummaraFadeInDuration));

        yield return new WaitForSeconds(4.5f);

        _ummaraEyes.Activate();

        StartText();

        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(2f);

        ShowNextParagraph();

        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(2f);

        InterruptTextFade();
        FadeOutText();

        ShadowTwinMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(1.5f);
        ShadowTwinMovement.obj.UnFreeze();

        yield return null;
    }

    public void CheckPoint1() {
        ShadowTwinMovement.obj.Freeze();
        StartCoroutine(CheckPoint1Coroutine());
    }

    private IEnumerator CheckPoint1Coroutine() {
        yield return new WaitForSeconds(1f);

        ShowNextParagraph();
        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(2f);

        InterruptTextFade();
        FadeOutText();

        _prisoner1.gameObject.SetActive(true);
        _prisoner2.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        _prisoner1.StartMoving();

        yield return new WaitForSeconds(1f);
        _prisoner1.Despawn();

        yield return new WaitForSeconds(1.5f);

        _prisoner2.StartMoving();

        yield return new WaitForSeconds(1f);
        _prisoner2.Despawn();

        yield return new WaitForSeconds(2f);
        ShadowTwinPlayer.obj.PlayBreathingOnKnees();

        yield return new WaitForSeconds(2f);

        ShowNextParagraph();

        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(2f);

        ShowNextParagraph();

        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(2f);

        InterruptTextFade();
        FadeOutText();

        ShadowTwinPlayer.obj.PlayGetUp();

        yield return new WaitForSeconds(1.5f);

        ShadowTwinMovement.obj.UnFreeze();
    }

    public void CheckPoint2() {
        StartCoroutine(CheckPoint2Coroutine());
    }

    private IEnumerator CheckPoint2Coroutine() {
        ShadowTwinMovement.obj.Freeze();

        yield return new WaitForSeconds(1f);

        MusicManager.obj.Stop();

        StartCoroutine(_ummaraBody.FadeOutSprite(_ummaraFadeOutDuration));
        StartCoroutine(_ummaraBody.FadeOutLight(_ummaraFadeOutDuration));
        StartCoroutine(_ummaraEyes.FadeOutSprites(_ummaraFadeOutDuration));

        yield return new WaitForSeconds(1f);
        ShadowTwinPlayer.obj.PlayLookUp();

        yield return new WaitForSeconds(1f);
        ShadowTwinPlayer.obj.PlayReachUpwards();

        yield return new WaitForSeconds(1f);

        ShowNextParagraph();

        yield return WaitForTypingToComplete();
        yield return new WaitForSeconds(1f);
        
        ShadowTwinPlayer.obj.PlayFallToTheGround();
        yield return new WaitForSeconds(1f);

        SoundFXManager.obj.Play2D(_teleport);

        GameManager.obj.IsPauseAllowed = false;
        PauseMenuManager.obj.SetPauseMusicInsteadOfMuffle(false);
        WhiteSceneFadeManager.obj.StartFadeOut(0.8f);

        yield return new WaitForSeconds(0.7f);

        while(WhiteSceneFadeManager.obj.IsFadingOut)
            yield return null;

        DustParticleMgr.obj.Enabled = true;
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
        PlayerPowersManager.obj.DeeCanForcePull = true;  

        //Before we load the room to teleport back to, make sure state is updated so we don't teleport back
        GameManager.obj.RegisterEvent(_dreamRoomCompleted);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_teleportBackToScene, LoadSceneMode.Additive);

        while(!asyncOperation.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
    }

    private void ShowNextParagraph() {
        DisplayNextParagraph();
    }

    private void StartText() {
        InitializeConversation(_dialogueContent);
        DisplayNextParagraph();
    }

    private void DisplayNextParagraph() {
        if(_paragraphs.Count == 0) {
            if(_conversationEnded && !_isTyping) {
                EndConversation();
                return;
            }
        }

        if(!_isTyping) {
            // Interrupt any ongoing fade and reset text to fully visible
            InterruptTextFade();
            ResetTextAlpha();
            
            try {
                DialogueContent.ParagraphEntry paragraphEntry = _paragraphs.Dequeue();
                p = paragraphEntry.text;
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
        for(int i = 0; i < dialogueContent.paragraphEntries.Count; ++i) {
            _paragraphs.Enqueue(dialogueContent.paragraphEntries[i]);
        }
    }

    private void EndConversation() {
        _paragraphs.Clear();
        _conversationEnded = false;
    }

    private void StartFadeOutTextAfterDelay() {
        if (_fadeOutTextCoroutine != null) {
            StopCoroutine(_fadeOutTextCoroutine);
        }
        _fadeOutTextCoroutine = StartCoroutine(FadeOutTextAfterDelay());
    }

    private IEnumerator FadeOutTextAfterDelay() {
        yield return new WaitForSeconds(_textFadeOutDelay);
        FadeOutText();
    }

    private void FadeOutText() {
        if (_tmpText != null) {
            _tmpText.DOFade(0f, _textFadeDuration);
        }
    }

    private void InterruptTextFade() {
        if (_fadeOutTextCoroutine != null) {
            StopCoroutine(_fadeOutTextCoroutine);
            _fadeOutTextCoroutine = null;
        }
        // Kill any ongoing DOTween fade animation
        if (_tmpText != null) {
            _tmpText.DOKill();
        }
    }

    private void ResetTextAlpha() {
        if (_tmpText != null) {
            Color color = _tmpText.color;
            color.a = 1f;
            _tmpText.color = color;
        }
    }

    private IEnumerator WaitForTypingToComplete() {
        while (_isTyping) {
            yield return null;
        }
    }

    void OnDestroy() {
        StopAllCoroutines();
        _typeWriter.onTextShowed.RemoveAllListeners();
        _tmpText?.DOKill();
        if(_ummara != null)
            _ummara.transform.DOKill();
    }
}
