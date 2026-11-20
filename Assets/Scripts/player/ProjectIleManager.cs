using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private const float DefaultHitRadiusMultiplier = 1f;

    public static ProjectileManager obj;
    public GameObject projectile;

    // Multiplies the hit radius of every projectile shot. Used to temporarily make force pushes easier to land, e.g. during boss fights.
    public float HitRadiusMultiplier { get; private set; } = DefaultHitRadiusMultiplier;

    private void Awake()
    {
        obj = this;
    }

    private void OnDestroy()
    {
        obj = null;
    }

    public void ShootProjectile(Vector3 spawnLocation, int horizontalDirection, float power, bool isPoweredUp)
    {
        GameObject projectilePrefab = Instantiate(this.projectile, spawnLocation, transform.rotation);
        Projectile projectileComponent = projectilePrefab.GetComponent<Projectile>();
        projectileComponent.ScaleHitRadius(HitRadiusMultiplier);
        projectileComponent.Shoot(horizontalDirection, power, isPoweredUp);
    }

    /// <summary>
    /// Sets the multiplier applied to the hit radius of projectiles shot from now on. Call ResetHitRadiusMultiplier to restore the default.
    /// </summary>
    public void SetHitRadiusMultiplier(float multiplier)
    {
        HitRadiusMultiplier = Mathf.Max(DefaultHitRadiusMultiplier, multiplier);
    }

    /// <summary>
    /// Restores the default projectile hit radius.
    /// </summary>
    public void ResetHitRadiusMultiplier()
    {
        HitRadiusMultiplier = DefaultHitRadiusMultiplier;
    }
}
