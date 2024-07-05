using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Room size: X: 40, Y: 22.5
public class RoomMgr : MonoBehaviour
{
    public GameObject virtualCamera;
    public SceneField currentScene;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
            virtualCamera.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !other.isTrigger)
        {
            virtualCamera.SetActive(false);
        }
    }
}
