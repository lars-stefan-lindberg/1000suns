using System.Collections;
using UnityEngine;

public class C27CutsceneManager : MonoBehaviour
{
    [SerializeField] private GameObject _caveAvatarFlyOffTarget;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(GameEventManager.obj.C27CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PlayerBlobMovement.obj.Freeze();
        GameEventManager.obj.IsPauseAllowed = false;
        
        yield return new WaitForSeconds(2f);

        CaveAvatar.obj.SetTarget(_caveAvatarFlyOffTarget.transform);

        yield return new WaitForSeconds(3f);

        PlayerBlobMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;
        GameEventManager.obj.C27CutsceneCompleted = true;
        yield return null;
    }
}
