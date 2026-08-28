using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CaveTentacleAnimatedTile", menuName = "Tiles/Cave Tentacle Animated Tile")]
public class CaveTentacleAnimatedTile : AnimatedTile
{
    // Dictionary to store per-tilemap speed multipliers
    private static System.Collections.Generic.Dictionary<Tilemap, float> speedMultipliers = new System.Collections.Generic.Dictionary<Tilemap, float>();

    public static void SetSpeedMultiplier(Tilemap tilemap, float multiplier)
    {
        speedMultipliers[tilemap] = multiplier;
    }

    public static void ClearSpeedMultiplier(Tilemap tilemap)
    {
        speedMultipliers.Remove(tilemap);
    }

    public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
    {
        // Get the base animation data from AnimatedTile
        bool hasAnimation = base.GetTileAnimationData(position, tilemap, ref tileAnimationData);
        
        if (hasAnimation && tilemap.GetComponent<Tilemap>() != null)
        {
            Tilemap tilemapComponent = tilemap.GetComponent<Tilemap>();
            
            // Apply the speed multiplier if one exists for this tilemap
            if (speedMultipliers.TryGetValue(tilemapComponent, out float multiplier))
            {
                tileAnimationData.animationSpeed *= multiplier;
            }
        }
        
        return hasAnimation;
    }
}
