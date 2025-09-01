using System.Collections;
using Cinemachine;
using DG.Tweening;
using FunkyCode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FirstRoomLoader : MonoBehaviour
{
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _fullRoomCamera;
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;
    [SerializeField] private Tilemap _tilemapRevealPath;
    private bool _dialogCompleted = false;

    void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
            _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        }
    }

    void Start()
    {
        if(!GameEventManager.obj.CaveLevelStarted) {
            _zoomedCamera.SetActive(true);
            CinemachineVirtualCamera zoomedCamera = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
            zoomedCamera.enabled = true;
            StartCoroutine(FadeInAndPlaySounds());
            StartCoroutine(AmbienceFadeIn());
            //Set darkness to dark
            LightingManager2D.Get().profile.DarknessColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            StartCoroutine(Cutscene());

            GameEventManager.obj.CaveLevelStarted = true;

            PlayerStatsManager.obj.ResumeTimer();

            if (_dialogueController != null)
            {
                _dialogueController.OnDialogueClosed += OnDialogueCompleted;
                _dialogueController.OnDialogueClosing += OnDialogueClosing;
            }
        } else {
            _tilemapRevealPath.color = new Color(_tilemapRevealPath.color.r, _tilemapRevealPath.color.g, _tilemapRevealPath.color.b, 0);  //If revisit first room make tilemap invisible
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            //Destroy(this, 15);
        }
    }

    private IEnumerator FadeInAndPlaySounds() {
        SoundFXManager.obj.PlayPlayerLongFall();
        yield return new WaitForSeconds(2.5f);
        SoundFXManager.obj.PlayPlayerLandHeavy();
        yield return new WaitForSeconds(2f);
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();

        yield return null;
    }

    private IEnumerator AmbienceFadeIn() {
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(2f);
        yield return null;
    }

    private IEnumerator Cutscene() {
        Player.obj.PlayGetUpAnimation();
        yield return new WaitForSeconds(8);
        Player.obj.StartAnimator();
        yield return new WaitForSeconds(6);

        _fullRoomCamera.SetActive(true);
        CinemachineVirtualCamera fullRoomCamera = _fullRoomCamera.GetComponent<CinemachineVirtualCamera>();
        CinemachineVirtualCamera zoomedCamera = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        fullRoomCamera.enabled = true;
        zoomedCamera.enabled = false;
        _zoomedCamera.SetActive(false);
        StartCoroutine(FadeInDarkness());

        //Zoom out time
        yield return new WaitForSeconds(4);

        //Show dialog
        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent);

        while (!_dialogCompleted) {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        DOTween.To(() => _tilemapRevealPath.color.a, x => _tilemapRevealPath.color = new Color(_tilemapRevealPath.color.r, _tilemapRevealPath.color.g, _tilemapRevealPath.color.b, x), 0, 1);
        SoundFXManager.obj.PlayBrokenFloorReappear(_tilemapRevealPath.transform);
        yield return new WaitForSeconds(0.5f);

        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;

        yield return null;
    }

    private IEnumerator FadeInDarkness() {
        yield return new WaitForSeconds(2);
        float fadeSpeed = 0.3f;
        Color defaultDarkness = new(0.33f, 0.33f, 0.33f, 1f);
        Color managerDarkness = LightingManager2D.Get().profile.DarknessColor;
        while (managerDarkness.r <= defaultDarkness.r - 0.02f) {
            Color color = Color.Lerp(managerDarkness, defaultDarkness, fadeSpeed * Time.deltaTime);
            LightingManager2D.Get().profile.DarknessColor = color;
            managerDarkness = LightingManager2D.Get().profile.DarknessColor;
            yield return null;
        }
        LightingManager2D.Get().profile.DarknessColor = defaultDarkness;
        yield return null;
    }


    private void OnDialogueClosing() {
        SoundFXManager.obj.PlayDialogueClose();
    }

    private void OnDialogueCompleted() {
        _dialogCompleted = true;
    }
}
