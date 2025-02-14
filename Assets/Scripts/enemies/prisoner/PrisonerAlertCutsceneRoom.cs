using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerAlertCutsceneRoom : MonoBehaviour
{
    public GameObject babyPrisoner;
    public GameObject prisoner;
    public GameObject lockedDoor;
    public GameObject lockedDoorOrb;
    public GameObject cutsceneTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(GameEventManager.obj.PrisonerIntroSeen) {
                babyPrisoner.SetActive(false);
                prisoner.SetActive(false);
                lockedDoor.SetActive(false);
                lockedDoorOrb.SetActive(false);
                cutsceneTrigger.SetActive(false);
            }
        }
    }
}
