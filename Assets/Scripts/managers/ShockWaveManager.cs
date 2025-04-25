using System.Collections;
using UnityEngine;

public class ShockWaveManager : MonoBehaviour
{
    public static ShockWaveManager obj;
    [SerializeField] private GameObject _shockWavePrefab;

    private Coroutine _shockWaveCoroutine;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    void Awake() {
        obj = this;
    }

    public void CallShockWave(Vector3 spawnLocation, float shockWaveTime, float startPosition, float endPosition) {
        GameObject shockWave = Instantiate(_shockWavePrefab, spawnLocation, Quaternion.identity);
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(shockWave, shockWaveTime, startPosition, endPosition));
    }

    private IEnumerator ShockWaveAction(GameObject shockWave, float shockWaveTime, float startPosition, float endPosition) {
        Material material = shockWave.GetComponent<SpriteRenderer>().material;
        material.SetFloat(_waveDistanceFromCenter, startPosition);

        float lerpedAmount;
        float elapsedTime = 0f;
        while(elapsedTime < shockWaveTime) {
            elapsedTime += Time.deltaTime;
            lerpedAmount = Mathf.Lerp(startPosition, endPosition, elapsedTime / shockWaveTime);
            material.SetFloat(_waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
        Destroy(shockWave);
    }

    void OnDestroy() {
        obj = null;
    }
}
