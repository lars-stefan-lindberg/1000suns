using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager obj;

    //Cutscene events
    public bool PrisonerIntroSeen { get; set; }
    public bool BabyPrisonerAlerted { get; set; }
    public bool CapePicked { get; set; }
    public bool FirstPowerUpPicked { get; set; }
    public bool PowerUpRoomsFloorBroken { get; set; }

    void Awake() {
        obj = this;
    }

    void Destroy() {
        obj = null;
    }
}
