using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C27CutsceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _caveAvatarFlyOffTarget;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(GameManager.obj.C27CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PlayerBlobMovement.obj.Freeze();
        GameManager.obj.IsPauseAllowed = false;
        
        yield return new WaitForSeconds(2f);

        CaveAvatar.obj.SetTarget(_caveAvatarFlyOffTarget.transform);

        yield return new WaitForSeconds(3f);

        PlayerBlobMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        GameManager.obj.C27CutsceneCompleted = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        yield return null;
    }
}
