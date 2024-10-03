using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashIntensity = 0.5f;
    [SerializeField] private float _defaultFlashSpeed = 0.25f;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _startFlashing = false;
    private bool _stopFlashing = false;
    private float _elapsedTime = 0f; //Elapsed time between a blend and non-blend
    private float _currentFlashAmount = 0f;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    public void StartFlashing() {
        _startFlashing = true;
        _elapsedTime = 0f;
    }

    void Update() {
        // if (_startFlashing) {
        //     StartCoroutine(SingleFlash());
        //     _startFlashing = false;
        // }
        if (_startFlashing) {
            if(_currentFlashAmount == 0f) {
                _elapsedTime = 0f;
            }
            
            _elapsedTime += Time.deltaTime;

            float flashTime = _defaultFlashSpeed;
            _currentFlashAmount = Mathf.Lerp(0f, _flashIntensity, _elapsedTime / flashTime);

            _material.SetFloat("_FlashAmount", _currentFlashAmount);

            if(_currentFlashAmount == _flashIntensity) {
                _startFlashing = false;
                _stopFlashing = true;
                _elapsedTime = 0;
            }
        } 
        else if(_stopFlashing) {
            if(_currentFlashAmount > 0f) {
                _elapsedTime += Time.deltaTime;
                _currentFlashAmount = Mathf.Lerp(_flashIntensity, 0f, (_elapsedTime / _defaultFlashSpeed));
                _material.SetFloat("_FlashAmount", _currentFlashAmount);
            } else 
                _stopFlashing = false;
        }
    }

    private IEnumerator SingleFlash() {
        _elapsedTime = 0f;
        while(_currentFlashAmount != _flashIntensity) {
            _elapsedTime += Time.deltaTime;
            _currentFlashAmount = Mathf.Lerp(0f, _flashIntensity, _elapsedTime / _defaultFlashSpeed);
            _material.SetFloat("_FlashAmount", _currentFlashAmount);
        }

        _elapsedTime = 0f;
        while(_currentFlashAmount != 0f) {
            _elapsedTime += Time.deltaTime;
            _currentFlashAmount = Mathf.Lerp(_flashIntensity, 0f, _elapsedTime / _defaultFlashSpeed);
            _material.SetFloat("_FlashAmount", _currentFlashAmount);
        }
        yield return null;
    }
}
