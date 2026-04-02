using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveInSurface : MonoBehaviour
{
    [SerializeField] private GameObject _breakableSurface;
    [SerializeField] private ParticleSystem _breakParticles;
    [SerializeField] private GameObject _ground;
    [SerializeField] private Animator _visibleTilemapAnimator;
    [SerializeField] private int _particleEmitCount = 50;
    [SerializeField] private ThunderLight _thunderLight;
    [SerializeField] private SceneField _firstCaveBackground;
    [SerializeField] private SceneField _firstCaveSurfaces;
    public EventReference _breakSfx;
    public EventReference _cracklingfx;

    private Animator _breakableSurfaceAnimator;
    private SpriteRenderer _breakableSurfaceRenderer;

    void Awake()
    {
        _breakableSurfaceAnimator = _breakableSurface.GetComponent<Animator>();
        _breakableSurfaceRenderer = _breakableSurface.GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if(collider.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            Activate();
        }
    }

    [ContextMenu("Activate cave-in")]
    private void Activate() {
        StartCoroutine(StartBreakSequence());
    }

    private IEnumerator StartBreakSequence() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);
        _thunderLight.Flash();
        yield return new WaitForSeconds(4f);
        _breakableSurfaceAnimator.SetTrigger("break");
    }

    public void OnBreakAnimationComplete() {
        SoundFXManager.obj.PlayAtPosition(_breakSfx, transform.position);
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 0.5f);
        _breakableSurfaceRenderer.enabled = false;
        _visibleTilemapAnimator.SetTrigger("reveal");
        _breakParticles.Emit(_particleEmitCount);
        StartCoroutine(DelayedDisableGround());
    }

    private IEnumerator DelayedDisableGround() {
        yield return new WaitForSeconds(0.1f);
        _ground.SetActive(false);
        yield return new WaitForSeconds(2f);
        StartCoroutine(LoadFirstCaveRoom());
    }

    private IEnumerator LoadFirstCaveRoom() {
        GameManager.obj.IsPauseAllowed = false;
        MusicManager.obj.Stop();
        AmbienceManager.obj.Stop();
        Player.obj.gameObject.SetActive(false);

        SceneFadeManager.obj.StartFadeOut(0.5f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());

        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_firstCaveBackground));
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstCaveSurfaces));

        AsyncOperation loadFirstCaveRoomOperation = SceneManager.LoadSceneAsync("Cave-1", LoadSceneMode.Additive);
        while(!loadFirstCaveRoomOperation.isDone) {
            yield return null;
        }
        Scene firstScene = SceneManager.GetSceneByName("Cave-1");
        SceneManager.SetActiveScene(firstScene);
        LevelManager.obj.LoadAdjacentRooms(firstScene);
        
        SceneManager.UnloadSceneAsync("Forest-1");
        SceneManager.UnloadSceneAsync("Forest-2");

        yield return null;
    }

    public void PlayCracklingSfx() {
        SoundFXManager.obj.PlayAtPosition(_cracklingfx, transform.position);
    }
}
