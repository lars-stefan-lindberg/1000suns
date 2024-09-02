using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager obj;

    private HashSet<string> pickedPowerUps;

    void Awake() {
        obj = this;
        pickedPowerUps = new HashSet<string>();
    }

    public void PowerUpPicked(string id) {
        pickedPowerUps.Add(id);
    }

    public bool IsPowerUpPicked(string id) {
        return pickedPowerUps.Contains(id);
    }

    void OnDestroy() {
        obj = null;
    }
}
