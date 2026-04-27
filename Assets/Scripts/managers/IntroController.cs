using System.Collections.Generic;
using UnityEngine;
using Febucci.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;
using TMPro;

public class IntroController : MonoBehaviour
{
    [SerializeField] private MusicTrack _introMusic;
    [SerializeField] private float _startMusicDuration = 2f;
    [SerializeField] private Canvas _textCanvas;
    [SerializeField] private TypewriterByCharacter _typeWriter;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private SceneField _introScene;
    [SerializeField] private float _textFadeDuration = 2f;
    [SerializeField] private SceneField _firstForestSurfaces;

    [Header("Ummara")]
    [SerializeField] private ParticleSystem _ummaraParticles;
    [SerializeField] private UmmaraEyesManager _ummaraEyes;
    [SerializeField] private UmmaraBodyManager _ummaraBody;
    [SerializeField] private GameObject _ummara;
    [SerializeField] private float _ummaraScaleDownDuration = 2f;
    [SerializeField] private Ease _ummaraScaleDownEase = Ease.InBack;

    [Header("World layers")]
    [SerializeField] private GameObject _cave;
    [SerializeField] private float _caveScaleDownDuration = 2f;
    [SerializeField] private Ease _caveScaleDownEase = Ease.InBack;
    [SerializeField] private SpriteGroupFader _caveSpriteGroup;
    [SerializeField] private float _caveFadeInDuration = 1f;
    [SerializeField] private GameObject _underworld;
    [SerializeField] private float _underworldScaleDownDuration = 2f;
    [SerializeField] private Ease _underworldScaleDownEase = Ease.InBack;
    [SerializeField] private SpriteGroupFader _underworldSpriteGroup;
    [SerializeField] private float _underworldFadeInDuration = 1f;
    [SerializeField] private GameObject _midnightZone;
    [SerializeField] private float _midnightZoneScaleDownDuration = 2f;
    [SerializeField] private Ease _midnightZoneScaleDownEase = Ease.InBack;
    [SerializeField] private SpriteGroupFader _midnightZoneSpriteGroup;
    [SerializeField] private float _midnightZoneFadeInDuration = 1f;
    [SerializeField] private GameObject _treeWorld;
    [SerializeField] private float _treeWorldScaleDownDuration = 2f;
    [SerializeField] private Ease _treeWorldScaleDownEase = Ease.InBack;
    [SerializeField] private SpriteGroupFader _treeWorldSpriteGroup;
    [SerializeField] private float _treeWorldFadeInDuration = 1f;

    [Header("Earth")]
    [SerializeField] private EarthContainer _earth;
    [SerializeField] private SpriteGroupFader _earthSpriteGroup;
    [SerializeField] private float _earthScaleDownDuration = 2f;
    [SerializeField] private float _earthScaleUpDuration = 8f;
    [SerializeField] private Ease _earthScaleDownEase = Ease.InBack;
    [SerializeField] private float _earthFadeInDuration = 1f;
    [SerializeField] private Transform _earthZoomTarget;
    [SerializeField] private float _earthZoomInTargetScale = 25f;
    [SerializeField] private Ease _earthScaleUpEase = Ease.InBack;

    [Header("Game logo")]
    [SerializeField] private Canvas _gameLogoCanvas;
    [SerializeField] private Animator _gameLogoAnimator;

    private Queue<DialogueContent.ParagraphEntry> _paragraphs = new();

    private bool _conversationEnded;
    private string p;
    private bool _isTyping;
    private TextMeshProUGUI _tmpText;
    private bool _testScale = false;
    private bool _firstForestRoomReady = false;

    void Start()
    {
        _gameLogoCanvas.worldCamera = Camera.main;
        _gameLogoCanvas.sortingLayerName = "UI";
        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene()
    {
        _caveSpriteGroup.SetAlpha(0);
        _underworldSpriteGroup.SetAlpha(0);
        _midnightZoneSpriteGroup.SetAlpha(0);
        _treeWorldSpriteGroup.SetAlpha(0);
        _earthSpriteGroup.SetAlpha(0);

        if(!_testScale) {
            SceneFadeManager.obj.SetFadedOutState();
            yield return new WaitForSeconds(_startMusicDuration);
            MusicManager.obj.Play(_introMusic);
            SceneFadeManager.obj.SetFadeInSpeed(0.2f);
            yield return new WaitForSeconds(3f);
            SceneFadeManager.obj.StartFadeIn();
            yield return new WaitForSeconds(5f);
            _ummaraEyes.Activate();
            yield return new WaitForSeconds(2f);
            StartText();
            yield return new WaitForSeconds(6f);
            DisplayNextParagraph();
            yield return new WaitForSeconds(10f);
            DisplayNextParagraph();
            yield return new WaitForSeconds(6f);
            FadeOutText();
            yield return new WaitForSeconds(4f);
            _ummaraParticles.Stop();
            yield return new WaitForSeconds(2f);
            _ummaraBody.Deactivate();
            _ummaraEyes.Deactivate();
            yield return new WaitForSeconds(0.5f);
        } else {
            _ummaraParticles.Stop();
            _ummaraBody.Deactivate();
            _ummaraEyes.Deactivate();
            yield return new WaitForSeconds(2f);
        }
        
        _ummara.transform.DOScale(Vector3.zero, _ummaraScaleDownDuration).SetEase(_ummaraScaleDownEase);
        yield return new WaitForSeconds(_ummaraScaleDownDuration - 0.5f);
        _ummara.SetActive(false);

        _underworld.SetActive(true);
        DOTween.To(() => 0f, alpha => _underworldSpriteGroup.SetAlpha(alpha), 1f, _underworldFadeInDuration);
        yield return new WaitForSeconds(_underworldFadeInDuration - 0.2f);
        _underworld.transform.DOScale(Vector3.zero, _underworldScaleDownDuration).SetEase(_underworldScaleDownEase);
        yield return new WaitForSeconds(_underworldScaleDownDuration - 0.6f);
        _underworld.SetActive(false);

        _midnightZone.SetActive(true);
        DOTween.To(() => 0f, alpha => _midnightZoneSpriteGroup.SetAlpha(alpha), 1f, _midnightZoneFadeInDuration);
        yield return new WaitForSeconds(_midnightZoneFadeInDuration - 0.2f);
        _midnightZone.transform.DOScale(Vector3.zero, _midnightZoneScaleDownDuration).SetEase(_midnightZoneScaleDownEase);
        yield return new WaitForSeconds(_midnightZoneScaleDownDuration - 0.6f);
        _midnightZone.SetActive(false);

        _treeWorld.SetActive(true);
        DOTween.To(() => 0f, alpha => _treeWorldSpriteGroup.SetAlpha(alpha), 1f, _treeWorldFadeInDuration);
        yield return new WaitForSeconds(_treeWorldFadeInDuration - 0.2f);
        _treeWorld.transform.DOScale(Vector3.zero, _treeWorldScaleDownDuration).SetEase(_treeWorldScaleDownEase);
        yield return new WaitForSeconds(_treeWorldScaleDownDuration - 0.6f);
        _treeWorld.SetActive(false);

        _cave.SetActive(true);
        DOTween.To(() => 0f, alpha => _caveSpriteGroup.SetAlpha(alpha), 1f, _caveFadeInDuration);
        yield return new WaitForSeconds(_caveFadeInDuration - 0.2f);
        _cave.transform.DOScale(Vector3.zero, _caveScaleDownDuration).SetEase(_caveScaleDownEase);
        yield return new WaitForSeconds(_caveScaleDownDuration - 0.6f);
        _cave.SetActive(false);

        _earth.gameObject.SetActive(true);
        DOTween.To(() => 0f, alpha => _earthSpriteGroup.SetAlpha(alpha), 1f, _earthFadeInDuration);
        yield return new WaitForSeconds(_earthFadeInDuration - 0.3f);
        _earth.transform.DOScale(Vector3.one, _earthScaleDownDuration).SetEase(_earthScaleDownEase);
        yield return new WaitForSeconds(0.5f);
        _earth.OnZoomOutStart();
        yield return new WaitForSeconds(_earthScaleDownDuration - 1.3f);
        _earth.OnZoomedOutCompleted();

        _gameLogoAnimator.SetTrigger("fadeIn");
        yield return new WaitForSeconds(15.6f);
        _gameLogoAnimator.SetTrigger("fadeOut");
        yield return new WaitForSeconds(4.7f);
        _earth.PrepZoomIn();
        yield return new WaitForSeconds(0.8f);
        
        Vector3 offset = _earthZoomTarget.position - _earth.transform.position;
        float targetScale = _earthZoomInTargetScale;
        Vector3 targetPosition = _earth.transform.position - (offset * (targetScale - _earth.transform.localScale.x));
        
        _earth.transform.DOScale(Vector3.one * targetScale, _earthScaleUpDuration).SetEase(_earthScaleUpEase);
        _earth.transform.DOMove(targetPosition, _earthScaleUpDuration).SetEase(_earthScaleUpEase);

        float waitForZoomInTime = 5f;
        yield return new WaitForSeconds(waitForZoomInTime);
        _earth.OnZoomInInProgress();

        yield return new WaitForSeconds(_earthScaleUpDuration - waitForZoomInTime - 2f);
        SceneFadeManager.obj.StartFadeOut(1f);
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadSceneAsync("Forest-1", LoadSceneMode.Additive);

        while (!_firstForestRoomReady)
        {
            yield return null;
        }

        _earth.OnZoomInCompleted();
        yield return new WaitForSeconds(2f);
        SceneManager.UnloadSceneAsync(_introScene.SceneName);
    }

    void Awake() {
        _textCanvas.worldCamera = Camera.main;
        _textCanvas.sortingLayerName = "UI";

        _tmpText = _textCanvas.GetComponentInChildren<TextMeshProUGUI>();

        _typeWriter.onTextShowed.AddListener(() => {
            _isTyping = false;
        });
        _typeWriter.onTypewriterStart.AddListener(() => {
            _isTyping = true;
        });
        
        IntroEvents.OnFirstForestRoomReady += OnFirstForestRoomReady;
    }
    
    private void OnFirstForestRoomReady()
    {
        _firstForestRoomReady = true;
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

    private void FadeOutText() {
        if (_tmpText != null) {
            _tmpText.DOFade(0f, _textFadeDuration);
        }
    }

    void OnDestroy() {
        _typeWriter.onTextShowed.RemoveAllListeners();
        _tmpText?.DOKill();
        _ummara?.transform.DOKill();
        IntroEvents.OnFirstForestRoomReady -= OnFirstForestRoomReady;
        IntroEvents.ClearFirstForestRoomReady();
    }
}
