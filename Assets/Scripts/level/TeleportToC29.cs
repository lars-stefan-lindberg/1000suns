using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToC29 : MonoBehaviour
{
    [SerializeField] private GameObject _shockWaveEmitter;
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private SceneField _teleportBackToScene;
    [SerializeField] private SceneField _thisScene;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(TeleportToC29Routine());
        }
    }

    private IEnumerator TeleportToC29Routine() {
        PlayerBlobMovement.obj.Freeze();
        //TODO: Set up the space room "music", including ending, in FMOD
        //MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 140, false);

        AmbienceManager.obj.Stop();

        SoundFXManager.obj.Play2D(_teleportSfx);
        SceneFadeManager.obj.StartWhiteFadeOut(0.8f);

        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        PlayerBlobMovement.obj.ToHuman(false);
        Destroy(_shockWaveEmitter);

        GameManager.obj.RegisterEvent(_dreamSequenceCompleted);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_teleportBackToScene, LoadSceneMode.Additive);

        while(!asyncOperation.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync(_thisScene.SceneName);

        yield return null;
    }
}
