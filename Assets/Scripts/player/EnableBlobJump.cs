using UnityEngine;

public class EnableBlobJump : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if(PlayerPowersManager.obj.BlobCanJump) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            PlayerPowersManager.obj.BlobCanJump = true;
            TutorialFooterManager.obj.StartFadeIn();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
