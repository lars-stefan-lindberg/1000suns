using System.Collections;
using UnityEngine;

public class EnableToHumanTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;
    private SpriteRenderer _renderer;
    private Animator _animator;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    private LightSprite2DFlicker _lightSprite2DFlicker;

    void Awake() {
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
        _lightSprite2DFlicker = GetComponent<LightSprite2DFlicker>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(PlayerPowersManager.obj.CanTurnFromBlobToHuman) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(Cutscene());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator Cutscene() {
        //Freeze player
        PlayerBlobMovement.obj.Freeze();
        GameEventManager.obj.IsPauseAllowed = false;

        yield return new WaitForSeconds(1);

        //Move player into correct position
        PlayerBlob.obj.transform.position = new Vector2(1308f, PlayerBlob.obj.transform.position.y);

        //Set "getting power" animation on blob
        PlayerBlob.obj.SetNewPower();

        //Start camera shake
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);
        yield return new WaitForSeconds(3);

        //Start crystal breaking
        _animator.SetTrigger("break");
        yield return new WaitForSeconds(1);
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
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;

        PlayerBlob.obj.SetNewPowerRecevied();

        yield return new WaitForSeconds(1.5f);

        PlayerBlobMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;

        PlayerPowersManager.obj.CanTurnFromBlobToHuman = true;
        yield return null;
    }

    public void EndOfBreak() {
        _renderer.enabled = false;
    }
}
