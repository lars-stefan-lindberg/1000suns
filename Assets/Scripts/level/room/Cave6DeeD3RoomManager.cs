using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave6DeeD3RoomManager : MonoBehaviour
{
    [SerializeField] private SceneField _teleportBackToScene;
    [SerializeField] private SceneField _teleportBackToAdjacentRoomScene1;
    [SerializeField] private SceneField _teleportBackToAdjacentRoomScene2;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private SceneField _otherScene;
    [SerializeField] private EventReference _teleport;
    [SerializeField] private GameEventId _crownPicked;

    public void Teleport() {
        ShadowTwinMovement.obj.Freeze();
        StartCoroutine(TeleportCoroutine());
    }

    private IEnumerator TeleportCoroutine() {
        GameManager.obj.IsPauseAllowed = false;
        GameManager.obj.RegisterEvent(_crownPicked);

        yield return new WaitForSeconds(0.3f);
        ShadowTwinMovement.obj.SetNewPower();
        
        yield return new WaitForSeconds(1f);

        ShadowTwinPull.obj.SetPullRangeGuideMode(ShadowTwinPull.PullRangeGuideMode.Off);

        AmbienceManager.obj.Stop();
        SoundFXManager.obj.Play2D(_teleport);
        WhiteSceneFadeManager.obj.StartFadeOut(0.8f);
        while(WhiteSceneFadeManager.obj.IsFadingOut)
            yield return null;

        DustParticleMgr.obj.Enabled = true;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_teleportBackToScene, LoadSceneMode.Additive);
        AsyncOperation asyncOperation2 = SceneManager.LoadSceneAsync(_teleportBackToAdjacentRoomScene1, LoadSceneMode.Additive);
        AsyncOperation asyncOperation3 = SceneManager.LoadSceneAsync(_teleportBackToAdjacentRoomScene2, LoadSceneMode.Additive);

        while(!asyncOperation.isDone)
            yield return null;


        while(!asyncOperation2.isDone)
            yield return null;

        while(!asyncOperation3.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
        SceneManager.UnloadSceneAsync(_otherScene.SceneName);
    }
}
