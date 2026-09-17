using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LocationMapping
{
    public string locationKey;
    public LocalizedString localizedString;
}

public class GameLocationManager : MonoBehaviour
{
    public static GameLocationManager obj;

    [Header("Location Mappings")]
    [SerializeField] private List<LocationMapping> _locationMappings = new List<LocationMapping>();

    private string _currentLocationKey;

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }

    // Set the current location by key
    public void SetCurrentLocation(string locationKey)
    {
        _currentLocationKey = locationKey;
    }

    // Get the current location key (for saving)
    public string GetCurrentLocationKey()
    {
        return _currentLocationKey;
    }

    // Get the localized string for a given location key
    public string GetLocalizedLocation(string locationKey)
    {
        if (string.IsNullOrEmpty(locationKey))
            return "";

        foreach (var mapping in _locationMappings)
        {
            if (mapping.locationKey == locationKey)
            {
                if (mapping.localizedString != null && !mapping.localizedString.IsEmpty)
                {
                    return mapping.localizedString.GetLocalizedString();
                }
            }
        }

        Debug.LogWarning($"No localized string found for location key: {locationKey}");
        return locationKey; // Fallback to key itself
    }

    // Get the localized string for the current location
    public string GetCurrentLocalizedLocation()
    {
        return GetLocalizedLocation(_currentLocationKey);
    }
}
