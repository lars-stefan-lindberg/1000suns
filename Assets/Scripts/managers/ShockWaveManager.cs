using System.Collections;
using UnityEngine;

public class ShockWaveManager : MonoBehaviour
{
    public static ShockWaveManager obj;
    [SerializeField] private GameObject _shockWavePrefab;
    [SerializeField] private float _shockWaveTime = 0.75f;

    private Coroutine _shockWaveCoroutine;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    void Awake() {
        obj = this;
    }

    public void CallShockWave(Vector3 spawnLocation) {
        GameObject shockWave = Instantiate(_shockWavePrefab, spawnLocation, Quaternion.identity);
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(shockWave, 0.05f, 0.15f));
    }

    private IEnumerator ShockWaveAction(GameObject shockWave, float startPosition, float endPosition) {
        Material material = shockWave.GetComponent<SpriteRenderer>().material;
        material.SetFloat(_waveDistanceFromCenter, startPosition);

        float lerpedAmount;
        float elapsedTime = 0f;
        while(elapsedTime < _shockWaveTime) {
            elapsedTime += Time.deltaTime;
            lerpedAmount = Mathf.Lerp(startPosition, endPosition, elapsedTime / _shockWaveTime);
            material.SetFloat(_waveDistanceFromCenter, lerpedAmount);

            yield return null;
        }
        Destroy(shockWave);
    }

    void OnDestroy() {
        obj = null;
    }
}
