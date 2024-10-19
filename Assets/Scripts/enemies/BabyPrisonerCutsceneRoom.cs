using UnityEngine;

public class BabyPrisonerCutsceneRoom : MonoBehaviour
{
    public GameObject babyPrisoner;
    public GameObject cutsceneTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(GameEventManager.obj.BabyPrisonerAlerted) {
                babyPrisoner.SetActive(false);
                cutsceneTrigger.SetActive(false);
            }
        }
    }
}
