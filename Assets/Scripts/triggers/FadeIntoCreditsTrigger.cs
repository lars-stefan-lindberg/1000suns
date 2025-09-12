using System.Collections;
using UnityEngine;

public class FadeIntoCreditsTrigger : MonoBehaviour
{
    [SerializeField] private GameObject creditsUI;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(FadeIntoCredits());
        }
    }

    private IEnumerator FadeIntoCredits() {
        SceneFadeManager.obj.StartFadeOut(0.5f);
        yield return new WaitForSeconds(3.5f);
        Player.obj.SetStatic();
        yield return new WaitForSeconds(1.5f);
        //Show credits
        creditsUI.SetActive(true);
        SceneFadeManager.obj.StartFadeIn();
    }
}
