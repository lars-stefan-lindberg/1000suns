using System.Collections;
using Cinemachine;
using UnityEngine;

public class C20CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _mainCamera;

    void OnTriggerEnter2D(Collider2D other) {
        if(GameManager.obj.C20CutsceneCompleted) {
            return;
        }
        if(other.CompareTag("Player")) {
            GameManager.obj.AfterPowerUpRoomsCompletedWallBreak = true;
            GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(0.5f);

        //Zoom in on player
        _zoomedCamera.SetActive(true);
        _mainCamera.SetActive(false);
        CinemachineVirtualCamera cinemachineVirtualCamera = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = true;

        yield return new WaitForSeconds(2.5f);

        //Grab moths
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(0.18f);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        yield return new WaitForSeconds(3f);

        //Zoom out
        cinemachineVirtualCamera.enabled = false;
        _zoomedCamera.SetActive(false);
        _mainCamera.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        //Show tutorial
        TutorialFooterManager.obj.StartFadeIn();

        yield return new WaitForSeconds(2f);

        //Unfreeze player
        PlayerMovement.obj.UnFreeze();

        //Set game event to prevent cutscene from showing again
        GameManager.obj.C20CutsceneCompleted = true;

        yield return null;
    }
}
