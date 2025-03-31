using UnityEngine;

public class GeneralTracker : MonoBehaviour
{
    public static GeneralTracker obj;

    public void TrackGameStarted() {
        Tracker.obj.TrackEvent("app_started");
    }

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }
}
