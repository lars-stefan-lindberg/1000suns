using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C26CutsceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _backgroundBlobs;
    [SerializeField] private Transform _sootFlyTarget1;
    [SerializeField] private Transform _sootFlyTarget2;
    [SerializeField] private Transform _sootFlyOffTarget;
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent1;
    [SerializeField] private DialogueContent _dialogueContent2;
    [SerializeField] private ParticleSystem _particleEffect;
    private int _dialogueIndex = 0;

    private bool _startCutscene = false;
    void OnTriggerEnter2D(Collider2D other) {
        if(GameManager.obj.C26CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            _startCutscene = true;
            GetComponent<BoxCollider2D>().enabled = false;
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

    void Start()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed += OnDialogueCompleted;
            _dialogueController.OnDialogueClosing += OnDialogueClosing;
        }
    }

    void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
            _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        }
    }

    private void OnDialogueClosing() {
        SoundFXManager.obj.PlayDialogueClose();
    }

    private void OnDialogueCompleted() {
        _dialogueIndex++;
    }

    void Update()
    {
        if(_startCutscene) {
            _startCutscene = false;
            GameManager.obj.IsPauseAllowed = false;
            PlayerMovement.obj.Freeze();
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        yield return new WaitForSeconds(1f);

        //Soot flies off
        CaveAvatar.obj.SetTarget(_sootFlyTarget1, 3f);

        yield return new WaitForSeconds(3f);

        CaveAvatar.obj.SetTarget(_sootFlyTarget2, 3f);

        yield return new WaitForSeconds(0.5f);

        //Set eye color
        CaveAvatar.obj.SetEyeColor(new Color(0.6226415f, 0.02643288f, 0.02643288f, 1f));
        SoundFXManager.obj.PlayCaveAvatarEvilEyesTransition();

        yield return new WaitForSeconds(1f);

        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent1);

        while(_dialogueIndex == 0) {
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        SoundFXManager.obj.PlayStartTransformingToBlobFirstTime(Camera.main.transform);

        //Loop through all children, get animators, and increase speed of animation
        Animator[] animators = _backgroundBlobs.GetComponentsInChildren<Animator>();
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        //Start particle effect
        _particleEffect.Play();
        StartCoroutine(GraduallyIncreaseParticleSpeed(_particleEffect, -2, -5, 2.3f, 1f, 7f));

        yield return new WaitForSeconds(0.8f);
        MusicManager.obj.PlayBlobTransform();
        yield return new WaitForSeconds(1.2f);

        SoundFXManager.obj.PlayC26Rumble();

        //Turn player into blob, slowly
        //Shake screen
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 6.5f);
        Player.obj.FlashFor(6.5f);
        Player.obj.SetAnimatorSpeed(0.03f);
        Player.obj.PlayToBlobAnimation();

        yield return new WaitForSeconds(11f);

        Player.obj.StartAnimator();

        //Loop through all children, get animators, and decrease speed of animation
        foreach(Animator animator in animators) {
            animator.speed = 5;
        }

        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent2);

        while(_dialogueIndex == 1) {
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        //Soot flies off
        CaveAvatar.obj.SetTarget(_sootFlyOffTarget, 5f);

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

        yield return new WaitForSeconds(2f);

        MusicManager.obj.PlayBlobRooms();
        PlayerBlobMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        GameManager.obj.C26CutsceneCompleted = true;
        PlayerPowersManager.obj.CanTurnFromHumanToBlob = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
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

    private IEnumerator GraduallyIncreaseParticleSpeed(ParticleSystem ps, float startSpeed, float endSpeed, float startLifetime, float endLifetime, float duration)
    {
        var main = ps.main;
        main.startSpeed = startSpeed;
        main.startLifetime = startLifetime;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
            float currentLifetime = Mathf.Lerp(startLifetime, endLifetime, t);

            main.startSpeed = currentSpeed;
            main.startLifetime = currentLifetime;
            yield return null;
        }

        main.startSpeed = endSpeed;
        main.startLifetime = endLifetime;

        ps.Stop();
    }
}
