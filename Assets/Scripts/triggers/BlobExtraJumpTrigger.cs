using System.Collections;
using FunkyCode;
using UnityEngine;

public class BlobExtraJumpTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    private LightSprite2DFlicker _lightSprite2DFlicker;

    void Awake() {
        if(PlayerPowersManager.obj.BlobCanExtraJump) {
            Destroy(gameObject);
            return;
        }
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
        _lightSprite2DFlicker = GetComponent<LightSprite2DFlicker>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(Cutscene());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator Cutscene() {
        MusicManager.obj.Pause();
        _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, 0.8f);
        //Freeze player
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze();
            yield return new WaitForSeconds(1f);
            Player.obj.PlayToBlobAnimation();
        } else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze();
        }
        GameEventManager.obj.IsPauseAllowed = false;

        yield return new WaitForSeconds(1);

        //Move player into correct position
        PlayerBlob.obj.transform.position = new Vector2(1308f, PlayerBlob.obj.transform.position.y);

        //Set "getting power" animation on blob
        PlayerBlob.obj.SetNewPower();

        //Start camera shake
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);
        yield return new WaitForSeconds(2.5f);

        //Start crystal breaking
        _animator.SetTrigger("break");
        yield return new WaitForSeconds(1.5f);
        //Fade out light
        _lightSprite2DFlicker.enabled = false;
        _lightSprite2DFadeManager.StartFadeOut();

        //Flash blob
        PlayerBlob.obj.FlashFor(2f);

        yield return new WaitForSeconds(3);

        //Start tutorial dialogue
        Time.timeScale = 0;
        _tutorialCanvas.SetActive(true);
        TutorialDialogManager.obj.StartFadeIn();
        SoundFXManager.obj.PlayPowerUpDialogueStinger();
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;

        PlayerBlob.obj.SetNewPowerRecevied();

        yield return new WaitForSeconds(1.5f);

        PlayerPowersManager.obj.BlobCanExtraJump = true;
        PlayerBlobMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;
        
        MusicManager.obj.Resume();

        StartCoroutine(IncreaseDarkness());

        yield return null;
    }

    public void EndOfBreak() {
        _renderer.enabled = false;
    }

    private IEnumerator IncreaseDarkness() {
        yield return new WaitForSeconds(2);
        Color startDarkness = LightingManager2D.Get().profile.DarknessColor;
        Color targetDarkness = new Color(0.015f, 0.015f, 0.015f, 1f);
        float duration = 3f;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            LightingManager2D.Get().profile.DarknessColor = Color.Lerp(startDarkness, targetDarkness, t);
            yield return null;
        }
        LightingManager2D.Get().profile.DarknessColor = targetDarkness;
        yield return null;
    }
}
