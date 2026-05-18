using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave35D2RoomManager : MonoBehaviour
{
    [SerializeField] private SceneField _teleportBackToScene;
    [SerializeField] private SceneField _teleportBackToAdjacentRoomScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private SceneField _otherScene;
    [SerializeField] private EventReference _teleport;

    public void Teleport() {
        PlayerMovement.obj.Freeze();
        StartCoroutine(TeleportCoroutine());
    }

    private IEnumerator TeleportCoroutine() {
        SoundFXManager.obj.Play2D(_teleport);
        SceneFadeManager.obj.StartWhiteFadeOut(0.8f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        DustParticleMgr.obj.Enabled = true;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_teleportBackToScene, LoadSceneMode.Additive);
        AsyncOperation asyncOperation2 = SceneManager.LoadSceneAsync(_teleportBackToAdjacentRoomScene, LoadSceneMode.Additive);

        while(!asyncOperation.isDone)
            yield return null;


        while(!asyncOperation2.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
        SceneManager.UnloadSceneAsync(_otherScene.SceneName);
    }
}
