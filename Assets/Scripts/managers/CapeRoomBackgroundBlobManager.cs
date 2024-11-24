using System.Collections;
using System.Linq;
using UnityEngine;

public class CapeRoomBackgroundBlobManager : MonoBehaviour
{
    public static CapeRoomBackgroundBlobManager obj;
    [SerializeField] private GameObject _tutorialCanvas;
    private bool _startCutscene = false;

    [ContextMenu("StartCutscene")]
    public void StartCutscene() {
        _startCutscene = true;
    }

    void Awake() {
        obj = this;
    }
    void Update()
    {
        if(_startCutscene) {
            _startCutscene = false;

            PlayerMovement.obj.Freeze();

            SoundFXManager.obj.PlayCapePickUp(Camera.main.transform);
            //Loop through all children, get animators, and increase speed of animation
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach(Animator animator in animators) {
                animator.speed = 10;
            }

            //Shake screen
            CameraShakeManager.obj.ShakeCamera(1.5f, 4f);

            //Turn background white, and then slowly fade back to black
            CapeRoomFadeManager.obj.StartFadeOut();
            StartCoroutine(FadeOutDelay());
            
            //Fade out blobs, show tutorial, and then play music
            StartCoroutine(FadeOutBlobSpritesAndTutorialAndPlayMusic());
        }
    }

    private IEnumerator FadeOutDelay() {
        yield return new WaitForSeconds(4);
        CapeRoomFadeManager.obj.StartFadeIn();
    }

    private IEnumerator FadeOutBlobSpritesAndTutorialAndPlayMusic() {
        yield return new WaitForSeconds(7);
        SpriteRenderer[] blobSprites = GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            foreach(SpriteRenderer blobSprite in blobSprites) {
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 3.5f * Time.deltaTime));
            }
            yield return null;
        }

        _tutorialCanvas.SetActive(true);
        TutorialFooterManager.obj.StartFadeIn();

        PlayerMovement.obj.UnFreeze();
        MusicManager.obj.PlayCaveSong();
    }

    void OnDestroy() {
        obj = null;
    }
}
