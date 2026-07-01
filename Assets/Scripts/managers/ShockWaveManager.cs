using System.Collections;
using UnityEngine;

public class ShockWaveManager : MonoBehaviour
{
    public static ShockWaveManager obj;
    [SerializeField] private GameObject _shockWavePrefab;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    void Awake() {
        obj = this;
    }

    public void CallShockWave(Vector3 spawnLocation, float shockWaveTime, float startPosition, float endPosition) {
        GameObject shockWave = Instantiate(_shockWavePrefab, spawnLocation, Quaternion.identity);
        SpriteRenderer spriteRenderer = shockWave.GetComponent<SpriteRenderer>();
        StartCoroutine(ShockWaveAction(shockWave, spriteRenderer, shockWaveTime, startPosition, endPosition));
    }

    private IEnumerator ShockWaveAction(GameObject shockWave, SpriteRenderer spriteRenderer, float shockWaveTime, float startPosition, float endPosition) {
        Material material = spriteRenderer.material;
        material.SetFloat(_waveDistanceFromCenter, startPosition);

        float elapsedTime = 0f;
        float inverseTime = 1f / shockWaveTime;
        while(elapsedTime < shockWaveTime) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * inverseTime;
            float lerpedAmount = Mathf.Lerp(startPosition, endPosition, t);
            material.SetFloat(_waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
        
        Destroy(material);
        Destroy(shockWave);
    }

    void OnDestroy() {
        obj = null;
    }
}
