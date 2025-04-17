using UnityEngine;

public class C30SetCaveAvatarStartingPos : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.C30CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            CaveAvatar.obj.SetStartingPositionInRoom30();
            CaveAvatar.obj.gameObject.SetActive(true);
        }
    }
}
