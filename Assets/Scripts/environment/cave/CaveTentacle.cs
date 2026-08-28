using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveTentacle : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private float speedMultiplier = 1.5f;

    private float currentSpeedMultiplier = 1f;

    void OnDisable()
    {
        // Reset speed when disabled
        if (tilemap != null)
        {
            CaveTentacleAnimatedTile.ClearSpeedMultiplier(tilemap);
            tilemap.RefreshAllTiles();
        }
    }

    public void IncreaseAnimatorSpeed() {
        if (tilemap == null) {
            Debug.LogWarning("Tilemap is not assigned in CaveTentacle");
            return;
        }

        // Increase the current speed multiplier
        currentSpeedMultiplier *= speedMultiplier;
        
        // Set the speed multiplier for this specific tilemap
        CaveTentacleAnimatedTile.SetSpeedMultiplier(tilemap, currentSpeedMultiplier);
        
        // Refresh all tiles to apply the new speed
        tilemap.RefreshAllTiles();
    }
}
