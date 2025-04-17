using System.Collections;
using FunkyCode;
using UnityEngine;

public class EnableBlobExtraJump : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if(PlayerPowersManager.obj.BlobCanExtraJump) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            PlayerPowersManager.obj.BlobCanExtraJump = true;
            GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(IncreaseDarkness());
        }
    }

    private IEnumerator IncreaseDarkness() {
        yield return new WaitForSeconds(2);
        float fadeSpeed = 0.1f;
        Color defaultDarkness = new(0.33f, 0.33f, 0.33f, 1f);
        Color managerDarkness = LightingManager2D.Get().profile.DarknessColor;
        while (managerDarkness.r <= defaultDarkness.r - 0.02f) {
            Color color = Color.Lerp(managerDarkness, defaultDarkness, fadeSpeed * Time.deltaTime);
            LightingManager2D.Get().profile.DarknessColor = color;
            managerDarkness = LightingManager2D.Get().profile.DarknessColor;
            yield return null;
        }
        LightingManager2D.Get().profile.DarknessColor = defaultDarkness;
        yield return null;
    }
}
