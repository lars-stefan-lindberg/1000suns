using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if(GameEventManager.obj.CapePicked) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
    void Update()
    {
        if(_startCutscene) {
            _startCutscene = false;

            GameEventManager.obj.IsPauseAllowed = false;
            PlayerMovement.obj.Freeze();
            StartCoroutine(FadeOutAndStopAmbience());

            SoundFXManager.obj.PlayCapePickUp(Camera.main.transform);
            //Loop through all children, get animators, and increase speed of animation
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach(Animator animator in animators) {
                animator.speed = 10;
            }

            //Shake screen
            CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4f);

            //Turn background white, and then slowly fade back to black
            WhiteFadeManager.obj.StartFadeOut();
            StartCoroutine(FadeOutDelay());
            
            //Fade out blobs, show tutorial, and then play music
            StartCoroutine(FadeOutBlobSpritesAndTutorialAndPlayMusic());
        }
    }

    private IEnumerator FadeOutAndStopAmbience() {
        float ambienceVolume = SoundMixerManager.obj.GetAmbienceVolume();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(1f, 0.001f));
        while(SoundMixerManager.obj.GetAmbienceVolume() > 0.001f) {
            yield return null;
        }
        //Give SoundMixerManager time to fully complete the fading
        yield return new WaitForSeconds(0.1f);
        AmbienceManager.obj.StopAmbience();
        SoundMixerManager.obj.SetAmbienceVolume(ambienceVolume);
    }

    private IEnumerator FadeOutDelay() {
        yield return new WaitForSeconds(4);
        WhiteFadeManager.obj.StartFadeIn();
    }

    private IEnumerator FadeOutBlobSpritesAndTutorialAndPlayMusic() {
        yield return new WaitForSeconds(2);
        Player.obj.transform.position = new Vector2(362.25f, Player.obj.transform.position.y);
        PlayerMovement.obj.SetNewPower();

        yield return new WaitForSeconds(5);
        
        SpriteRenderer[] blobSprites = GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            for (int i = 0; i < blobSprites.Length; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 3.5f * Time.deltaTime));
            }
            yield return null;
        }

        PlayerMovement.obj.Freeze();
        Time.timeScale = 0;
        _tutorialCanvas.SetActive(true);
        TutorialDialogManager.obj.StartFadeIn();
        SoundFXManager.obj.PlayPowerUpDialogueStinger();
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;

        PlayerMovement.obj.SetNewPowerRecevied();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();

        MusicManager.obj.PlayCaveFirstSong();
        
        //Make sure events, powers, and music is saved if reloading the room
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        GameEventManager.obj.IsPauseAllowed = true;
    }

    void OnDestroy() {
        obj = null;
    }
}
