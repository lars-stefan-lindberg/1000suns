using System.Collections;
using UnityEngine;

public class Forest2GlyphTriggeredManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GlyphStone _glyphStone;
    [SerializeField] private GameObject _rainSystems;
    [SerializeField] private ThunderLight _thunderLight;
    [SerializeField] private AmbienceTrack _rain;
    [SerializeField] private GameEventId _forestGlyphTouchEventId;
    [SerializeField] private GameObject _cutsceneCamera;

    private Coroutine _cutsceneCoroutine;

    void Start() {
        PlayerEvents.OnForestGlyphTouched += Activate;
    }

    public void InitiateEliTouchAnimation() {
        if(GameManager.obj.HasEvent(_forestGlyphTouchEventId)) {
            return;
        }

        PlayerMovement.obj.Freeze();
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        _cutsceneCoroutine = StartCoroutine(StartCutscene());
    }

    public void Activate() {
        _glyphStone.Activate();
        _rainSystems.SetActive(true);

        _thunderLight.Flash();
        
        AmbienceManager.obj.Play(_rain);
        //Play music
    }

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);
        yield return new WaitForSeconds(0.5f);
        _cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2.5f);

        PlayerMovement.obj.StartWalking();
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));

        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.StopWalking();

        Player.obj.PlayForestGlyphTouch(); //The animation contains an event trigger for PlayerEvents.OnForestGlyphTouched

        yield return new WaitForSeconds(10.5f);
        _cutsceneCamera.SetActive(false);

        PauseMenuManager.obj.UnregisterSkippable();
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_forestGlyphTouchEventId);
    }

    public void RequestSkip() {
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        StopCoroutine(_cutsceneCoroutine);
        PlayerMovement.obj.StopWalking();

        AmbienceManager.obj.Play(_rain);
        _rainSystems.SetActive(true);
        _thunderLight.Stop();
        Player.obj.ResetAnimator();
        Player.obj.transform.position = new Vector2(129.5f, -22.875f);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        _glyphStone.Activate();
        _cutsceneCamera.SetActive(false);

        GameManager.obj.RegisterEvent(_forestGlyphTouchEventId);
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    void OnDestroy() {
        PlayerEvents.OnForestGlyphTouched -= Activate;
    }
}
