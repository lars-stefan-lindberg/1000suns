using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayRegularCaveAmbienceTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(PlayRegularCaveAmbience());
        }
    }

    private IEnumerator PlayRegularCaveAmbience() {
        if(!AmbienceManager.obj.IsAmbienceSource1Playing()) {
            AmbienceManager.obj.PlayCaveAmbience();
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            AmbienceManager.obj.FadeInAmbienceSource1(1.5f);
            yield return new WaitForSeconds(1f);
            if(AmbienceManager.obj.IsAmbienceSource2Playing()) {
                AmbienceManager.obj.FadeOutAmbienceSource2And3(1f);
            }
        }
    }
}
