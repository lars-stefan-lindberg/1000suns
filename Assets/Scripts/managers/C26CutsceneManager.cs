using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class C26CutsceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _backgroundBlobs;
    [SerializeField] private Transform _sootFlyOffTarget;
    private bool _startCutscene = false;
    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.C26CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            _startCutscene = true;
        }
    }

    void Awake()
    {
        //Start by setting all blobs transparent, so they can be faded in later
        SpriteRenderer[] blobRenderers = _backgroundBlobs.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer blobRenderer in blobRenderers) {
            blobRenderer.color = new Color(blobRenderer.color.r, blobRenderer.color.b, blobRenderer.color.g, 0);
        }
    }

    void Update()
    {
        if(_startCutscene) {
            _startCutscene = false;
            GameEventManager.obj.IsPauseAllowed = false;
            PlayerMovement.obj.Freeze();
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        yield return new WaitForSeconds(2f);

        //Loop through all children, get animators, and increase speed of animation
        Animator[] animators = _backgroundBlobs.GetComponentsInChildren<Animator>();
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        //Turn player into blob
        Player.obj.PlayToBlobAnimation();
        yield return new WaitForSeconds(1f);
        PlayerBlobMovement.obj.Freeze();

        yield return new WaitForSeconds(2f);
        
        //Soot flies off
        CaveAvatar.obj.SetTarget(_sootFlyOffTarget);

        yield return new WaitForSeconds(2f);

        //When finished, fade out blobs
        SpriteRenderer[] blobSprites = _backgroundBlobs.GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            for (int i = 0; i < blobSprites.Length; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 2.5f * Time.deltaTime));
            }
            yield return null;
        }

        //Shake screen
        //CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 5f);

        PlayerBlobMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;
        GameEventManager.obj.C26CutsceneCompleted = true;
        PlayerPowersManager.obj.CanTurnFromHumanToBlob = true;
        yield return null;
    }

    public void FadeInBlobs() {
        StartCoroutine(FadeInBlobSprites());
    }

    private IEnumerator FadeInBlobSprites() {
        SpriteRenderer[] blobSprites = _backgroundBlobs.GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a < 1) {
            for (int i = 0; i < blobSprites.Length; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 1, 0.5f * Time.deltaTime));
            }
            yield return null;
        }
    }
}
