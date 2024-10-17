using System.Collections;
using System.Linq;
using UnityEngine;

public class EndRoomBackgroundBlobManager : MonoBehaviour
{
    public static EndRoomBackgroundBlobManager obj;
    [SerializeField] private GameObject tempPlayerObj;
    [SerializeField] private GameObject creditsUI;
    [SerializeField] private Transform playerMoveTarget;
    private bool _startCutscene = false;

    [ContextMenu("StartCutscene")]
    public void StartCutscene() {
        _startCutscene = true;
    }

    void Awake() {
        obj = this;
        //Start by setting all blobs transparent, so they can be faded in later
        SpriteRenderer[] blobRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer blobRenderer in blobRenderers) {
            blobRenderer.color = new Color(blobRenderer.color.r, blobRenderer.color.b, blobRenderer.color.g, 0);
        }
    }
    void Update()
    {
        if(_startCutscene) {
            _startCutscene = false;

            //Play big blob sound

            //Loop through all children, get animators, and increase speed of animation
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach(Animator animator in animators) {
                animator.speed = 10;
            }

            //Shake screen
            CameraShakeManager.obj.ShakeCamera(1.5f, 5f);

            //Start end song
            MusicManager.obj.PlayEndSong();

            //Player cutscene
            StartCoroutine(PlayerFadeAway());
            
            //Fade out blobs and show credits
            StartCoroutine(FadeOutBlobSpritesAndShowCredits());
        }
    }

    public void FadeInBlobs() {
        StartCoroutine(FadeInBlobSprites());
    }

    private IEnumerator FadeInBlobSprites() {
        SpriteRenderer[] blobSprites = GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a < 1) {
            foreach(SpriteRenderer blobSprite in blobSprites) {
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 1, 0.5f * Time.deltaTime));
            }
            yield return null;
        }
    }

    private IEnumerator FadeOutBlobSpritesAndShowCredits() {
        yield return new WaitForSeconds(14);
        SpriteRenderer[] blobSprites = GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            foreach(SpriteRenderer blobSprite in blobSprites) {
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 2.5f * Time.deltaTime));
            }
            yield return null;
        }
        yield return new WaitForSeconds(5f);
        //Fade out screen
        SceneFadeManager.obj.StartFadeOut(0.8f);
        yield return new WaitForSeconds(5f);
        //Show credits
        creditsUI.SetActive(true);
        SceneFadeManager.obj.StartFadeIn();
    }

    private IEnumerator PlayerFadeAway() {
        PlayerMovement.obj.Freeze(3f);
        yield return new WaitForSeconds(4f);
        Player.obj.gameObject.SetActive(false);
        tempPlayerObj.SetActive(true);
        while(tempPlayerObj.transform.position != playerMoveTarget.position) {
            tempPlayerObj.transform.position = Vector2.MoveTowards(tempPlayerObj.transform.position, playerMoveTarget.position, 1.5f * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        SpriteRenderer tempPlayerRenderer = tempPlayerObj.GetComponent<SpriteRenderer>();
        while(tempPlayerRenderer.color.a > 0) {
            tempPlayerRenderer.color = new Color(tempPlayerRenderer.color.r, tempPlayerRenderer.color.b, tempPlayerRenderer.color.g, Mathf.MoveTowards(tempPlayerRenderer.color.a, 0, 0.5f * Time.deltaTime));
            yield return null;
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
