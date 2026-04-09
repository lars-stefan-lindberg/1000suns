using System.Collections;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstForestRoomLoader : MonoBehaviour
{
    [SerializeField] private GameEventId _eliFirstForestRoomLoaded;
    [SerializeField] private AmbienceTrack _ambience;
    [SerializeField] private TentCutsceneManager _tentCutsceneManager;
    [SerializeField] private GameObject _zoomedCamera;

    void Start() {
        if(!GameManager.obj.HasEvent(_eliFirstForestRoomLoaded)) {
            StartCoroutine(LoadRoom());
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
        }
    }

    private IEnumerator LoadRoom() {
        Player.obj.SetForestStartingCoordinates();
        PlayerMovement.obj.spriteRenderer.enabled = false;
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerMovement.obj.Freeze();

        CaveAvatar.obj.gameObject.SetActive(false);

        AmbienceManager.obj.Play(_ambience);
        yield return StartCoroutine(FadeInScreenAndStart());

        GameManager.obj.RegisterEvent(_eliFirstForestRoomLoaded);

        yield return null;
    }

    private IEnumerator FadeInScreenAndStart() {
        yield return new WaitForSeconds(2f); //Give title screen time to unload
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();

        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        _tentCutsceneManager.StartTentSequence();

        yield return null;
    }
}
