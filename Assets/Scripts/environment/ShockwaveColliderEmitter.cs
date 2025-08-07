using UnityEngine;

public class ShockwaveColliderEmitter : MonoBehaviour
{
    public GameObject shockwavePrefab;

    [Header("Shockwave Timing Settings")]
    public float minInterval = 0.5f;
    public float maxInterval = 2.0f;
    public float minPlayerDistance = 3.0f;
    public float maxPlayerDistance = 15.0f;
    public float minForce = 20f;
    public float maxForce = 60f;
    private float timer = 0f;

    private float GetDynamicInterval(float playerDist)
    {
        if (playerDist >= maxPlayerDistance)
            return maxInterval;
        if (playerDist <= minPlayerDistance)
            return 0f; // special value: disables shockwave
        float t = (playerDist - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
        return Mathf.Lerp(minInterval, maxInterval, t);
    }

    private float GetDynamicForce(float playerDist)
    {
        if (playerDist >= maxPlayerDistance)
            return maxForce;
        if (playerDist <= minPlayerDistance)
            return 0f; // special value: disables shockwave
        float t = (playerDist - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
        return Mathf.Lerp(minForce, maxForce, t);
    }

    [ContextMenu("Trigger Shockwave")]
    void TriggerShockwave(float playerDist)
    {
        ShockWaveManager.obj.CallShockWave(transform.position, 0.8f, 0.05f, 1f);

        SoundFXManager.obj.PlayPlayerStatueShockWave(transform);

        CameraShakeManager.obj.ForcePushShake();

        float force = GetDynamicForce(playerDist);
        var shockwaveGameObject = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        var shockwave = shockwaveGameObject.GetComponent<ShockwaveCollider>();
        if (shockwave != null)
            shockwave.force = force;
    }

    void FixedUpdate()
    {
        if (PlayerManager.obj == null || PlayerManager.obj.GetPlayerTransform() == null)
            return;
        float playerDist = Vector3.Distance(transform.position, PlayerManager.obj.GetPlayerTransform().position);
        float dynamicInterval = GetDynamicInterval(playerDist);
        if (dynamicInterval == 0f)
        {
            timer = 0f; // Reset timer if player is too close
            return;
        }
        timer += Time.deltaTime;
        if (timer >= dynamicInterval)
        {
            TriggerShockwave(playerDist);
            timer = 0f;
        }
    }
}
