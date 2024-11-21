using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Unity.VisualScripting;
using System.Linq;

//Room size: X: 40, Y: 22.5
public class RoomMgr : MonoBehaviour
{
    public GameObject virtualCamera;
    public GameObject followCamera;
    public GameObject alternativeCamera;
    public SceneField currentScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            bool enterFromRight = transform.position.x < other.gameObject.transform.position.x;
            if(alternativeCamera != null && enterFromRight) {
                alternativeCamera.SetActive(true);
                CinemachineVirtualCamera cinemachineVirtualCamera = alternativeCamera.GetComponent<CinemachineVirtualCamera>();
                cinemachineVirtualCamera.enabled = true;
            } else {
                ActivateVirtualCamera();
            }

            GameObject[] sceneGameObjects = SceneManager.GetSceneByName(currentScene).GetRootGameObjects();
            IEnumerable<GameObject> prisonerGameObjects = sceneGameObjects.Where(gameObject => gameObject.CompareTag("Enemy"));
            foreach(GameObject gameObject in prisonerGameObjects) {
                Prisoner prisoner = gameObject.GetComponent<Prisoner>();
                prisoner.offScreen = false;
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCamera.SetActive(false);
            virtualCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
            if(alternativeCamera != null) {
                alternativeCamera.SetActive(false);
                alternativeCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
            }
            if(followCamera != null) {
                followCamera.SetActive(false);
                followCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
            }

            GameObject[] sceneGameObjects = SceneManager.GetSceneByName(currentScene).GetRootGameObjects();
            IEnumerable<GameObject> prisonerGameObjects = sceneGameObjects.Where(gameObject => gameObject.CompareTag("Enemy"));
            foreach(GameObject gameObject in prisonerGameObjects) {
                Prisoner prisoner = gameObject.GetComponent<Prisoner>();
                prisoner.offScreen = true;
            }
        }
    }

    public void ActivateVirtualCamera() {
        virtualCamera.SetActive(true);
        CinemachineVirtualCamera cinemachineVirtualCamera = virtualCamera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = true;
    }
}
