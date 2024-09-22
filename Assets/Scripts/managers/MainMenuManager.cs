using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SceneField _persistentGameplay;
    [SerializeField] private SceneField _caveRoom1;
    [SerializeField] private SceneField _titleScreen;

    [SerializeField] private Image _fadeOutImage;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;

    [SerializeField] private Color _fadeOutStartColor;

    public bool IsFadingOut { get; private set; }

    void Update() {
        if(IsFadingOut) {
            if(_fadeOutImage.color.a < 1f) {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            } else {
                IsFadingOut = false;
            }
        }
    }

    public void StartGame() {
        StartCoroutine(EnableCameraAfterLoad());
    }

    private IEnumerator EnableCameraAfterLoad() {
        StartFadeOut();
        while(IsFadingOut)
            yield return null;

        SceneManager.LoadSceneAsync(_persistentGameplay, LoadSceneMode.Additive);
        Scene persistentGameplay = SceneManager.GetSceneByName(_persistentGameplay.SceneName);
        while(!persistentGameplay.isLoaded) {
            yield return null;
        }
        SceneManager.LoadSceneAsync(_caveRoom1, LoadSceneMode.Additive);
        Scene caveRoom1 = SceneManager.GetSceneByName(_caveRoom1.SceneName);
        while(!caveRoom1.isLoaded) {
            yield return null;
        }
        
        GameObject[] sceneGameObjects = caveRoom1.GetRootGameObjects();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        RoomMgr roomMgr = room.GetComponentInChildren<RoomMgr>();
        roomMgr.ActivateVirtualCamera();

        SceneManager.UnloadSceneAsync(_titleScreen.SceneName);
    }

    private void StartFadeOut() {
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }
}
