using System.Collections;
using UnityEngine;

public class ShockWaveManager : MonoBehaviour
{
    public static ShockWaveManager obj;
    [SerializeField] private GameObject _shockWavePrefab;
    [SerializeField] private GameObject _bigShockWavePrefab;

    private static int _waveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");
    private Coroutine _shockwaveActionCoroutine;
    private Material _currentMaterial;
    private GameObject _currentShockWave;

    void Awake() {
        obj = this;
    }

    public void CallShockWave(Vector3 spawnLocation, float shockWaveTime, float startPosition, float endPosition) {
        GameObject shockWave = Instantiate(_shockWavePrefab, spawnLocation, Quaternion.identity);
        SpriteRenderer spriteRenderer = shockWave.GetComponent<SpriteRenderer>();
        _shockwaveActionCoroutine = StartCoroutine(ShockWaveAction(shockWave, spriteRenderer, shockWaveTime, startPosition, endPosition));
    }

    public void CallBigShockWave(Vector3 spawnLocation, float shockWaveTime, float startPosition, float endPosition) {
        GameObject shockWave = Instantiate(_bigShockWavePrefab, spawnLocation, Quaternion.identity);
        SpriteRenderer spriteRenderer = shockWave.GetComponent<SpriteRenderer>();
        _shockwaveActionCoroutine = StartCoroutine(ShockWaveAction(shockWave, spriteRenderer, shockWaveTime, startPosition, endPosition));
    }

    private IEnumerator ShockWaveAction(GameObject shockWave, SpriteRenderer spriteRenderer, float shockWaveTime, float startPosition, float endPosition) {
        _currentMaterial = spriteRenderer.material;
        _currentShockWave = shockWave;
        var material = _currentMaterial;
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

    public void DestroyShockwave() {
        if(_shockwaveActionCoroutine != null) {
            StopCoroutine(_shockwaveActionCoroutine);
            _shockwaveActionCoroutine = null;
        }
        if(_currentShockWave != null) {
            Destroy(_currentShockWave);
            _currentShockWave = null;
        }
        if(_currentMaterial != null) {
            Destroy(_currentMaterial);
            _currentMaterial = null;
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
