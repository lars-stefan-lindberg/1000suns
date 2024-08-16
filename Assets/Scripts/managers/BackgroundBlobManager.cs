using System.Collections;
using System.Linq;
using UnityEngine;

public class BackgroundBlobManager : MonoBehaviour
{
    public static BackgroundBlobManager obj;
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
            
            //Fade out blobs
            StartCoroutine(FadeOutBlobSprites());

            //Create temporary cape trigger that will start the cutscene

            //Play big blob sound
        }
    }

    private IEnumerator FadeOutDelay() {
        yield return new WaitForSeconds(4);
        CapeRoomFadeManager.obj.StartFadeIn();
    }

    private IEnumerator FadeOutBlobSprites() {
        yield return new WaitForSeconds(5);
        SpriteRenderer[] blobSprites = GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            foreach(SpriteRenderer blobSprite in blobSprites) {
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 3.5f * Time.deltaTime));
            }
            yield return null;
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
