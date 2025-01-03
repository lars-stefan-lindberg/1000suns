using System.Collections;
using System.Collections.Generic;
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

    public void shootProjectile(Vector3 spawnLocation, int horizontalDirection, float power, bool isPoweredUp)
    {
        GameObject projectilePrefab = Instantiate(this.projectile, spawnLocation, transform.rotation);
        projectilePrefab.GetComponent<Projectile>().Shoot(horizontalDirection, power, isPoweredUp);
    }
}
