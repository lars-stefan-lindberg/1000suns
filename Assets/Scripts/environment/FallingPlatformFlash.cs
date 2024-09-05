using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashIntensity = 0.5f;
    [SerializeField] private float _defaultFlashSpeed = 0.25f;
    private float _mediumFlashSpeed;
    private float _fastFlashSpeed;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _startFlashing = false;
    private float _elapsedTime = 0f; //Elapsed time between a blend and non-blend
    private float _totalElapsedTime = 0f;
    private float _currentFlashAmount = 0f;
    private float _mediumFlashTime;
    private float _fastFlashTime;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _mediumFlashSpeed = _defaultFlashSpeed * (2f / 3f);
        _fastFlashSpeed = _defaultFlashSpeed * (1f / 3f);
    }

    public void StartFlashing(float timeBeforeFall) {
        _startFlashing = true;
        _mediumFlashTime = timeBeforeFall / 2f;
        _fastFlashTime = timeBeforeFall * (2f / 3f);
    }

    public void StopFlashing() {
        _startFlashing = false;
    }

    private bool blended = false;
    void Update() {
        if (_startFlashing) {
            if(_currentFlashAmount == 0f) {
                _elapsedTime = 0f;
                blended = false;
            }
            else if(_currentFlashAmount == _flashIntensity) {
                _elapsedTime = 0f;
                blended = true;
            }

            _elapsedTime += Time.deltaTime;
            _totalElapsedTime += Time.deltaTime;

            float flashTime = CalculateFlashTime();
            if(blended)
                _currentFlashAmount = Mathf.Lerp(_flashIntensity, 0f, _elapsedTime / flashTime);
            else
                _currentFlashAmount = Mathf.Lerp(0f, _flashIntensity, _elapsedTime / flashTime);

            _material.SetFloat("_FlashAmount", _currentFlashAmount);
        } 
        else {
            if(_currentFlashAmount > 0f) {
                _elapsedTime += Time.deltaTime;
                _currentFlashAmount = Mathf.Lerp(_flashIntensity, 0f, (_elapsedTime / _defaultFlashSpeed));
                _material.SetFloat("_FlashAmount", _currentFlashAmount);
            }
        }
    }

    //Using three different levels of flashing - Slow, medium, high
    private float CalculateFlashTime() {
        if(_totalElapsedTime >= _mediumFlashTime && _totalElapsedTime < _fastFlashTime)
            return _mediumFlashSpeed;
        else if(_totalElapsedTime >= _fastFlashTime)
            return _fastFlashSpeed;

        //Default use "slow", default flash time
        return _defaultFlashSpeed;
    }
}
