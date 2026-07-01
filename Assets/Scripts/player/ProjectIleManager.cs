using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager obj;
    public GameObject projectile;

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
        projectileComponent.Shoot(horizontalDirection, power, isPoweredUp);
    }
}
