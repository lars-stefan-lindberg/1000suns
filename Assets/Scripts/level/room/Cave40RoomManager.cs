using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave40RoomManager : MonoBehaviour
{
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private SpriteFlash _elevatorFlash;
    [SerializeField] private SceneField _nextScene;
    [SerializeField] private SceneField _thisScene;

    public void StartElevator() {
        StartCoroutine(StartElevatorCoroutine());
    }

    private IEnumerator StartElevatorCoroutine() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1.5f);
        _elevatorFlash.Flash();
        yield return new WaitForSeconds(1.5f);
        _elevator.StartMoving();

        yield return new WaitForSeconds(3f);

        SceneFadeManager.obj.StartFadeOut(0.8f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        //Load next scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        
        //Unload this scene
        SceneManager.UnloadSceneAsync(_thisScene);
    }
}
