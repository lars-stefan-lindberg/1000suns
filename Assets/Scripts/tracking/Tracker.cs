using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public static Tracker obj;

    private readonly bool _isTracking = false;

    public void TrackEvent(string eventName, Dictionary<string, object> props = null)
    {
        if (_isTracking) {
            AptabaseSDK.Aptabase.TrackEvent(eventName, props);
            Debug.Log($"Sending event to Aptabase. Event: {eventName}, Props: {props}");
        }
    }

    void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
