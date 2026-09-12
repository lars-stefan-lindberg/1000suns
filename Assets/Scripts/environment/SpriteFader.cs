using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFader : MonoBehaviour
{
    [SerializeField] private float _fadeOutCompletelyThreshold = 2;
    [SerializeField] private float _fadeSlightlyThreshold = -2;
    [SerializeField] private float _fadeSlightlyAlpha = 0.8f;
    [SerializeField] private float _fadeSpeed = 1f;
    [SerializeField] private float _checkInterval = 0.2f;
    [SerializeField] private Transform _fadeOutLeftBoundary;
    [SerializeField] private Transform _fadeOutRightBoundary;
    
    private SpriteRenderer _renderer;
    private float _timer = 0f;
    private Coroutine _fadeCoroutine;
    private float _targetAlpha = 1f;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        
        if (_timer >= _checkInterval)
        {
            _timer = 0f;
            CheckPlayerPosition();
        }
    }

    private void CheckPlayerPosition()
    {
        var playerType = PlayerManager.obj.GetActivePlayerType();
        var playerTransform = PlayerManager.obj.GetPlayerTransform(playerType);
        var playerVerticalWorldPosition = playerTransform.position.y;
        var playerHorizontalWorldPosition = playerTransform.position.x;
        var objectVerticalPosition = transform.position.y;
        
        float relativePosition = playerVerticalWorldPosition - objectVerticalPosition;
        float newTargetAlpha;
        
        if (relativePosition < _fadeSlightlyThreshold)
        {
            newTargetAlpha = 1f;
        }
        else if (relativePosition < _fadeOutCompletelyThreshold)
        {
            bool isWithinHorizontalBounds = IsPlayerWithinHorizontalBounds(playerHorizontalWorldPosition);
            
            if (isWithinHorizontalBounds)
            {
                newTargetAlpha = _fadeSlightlyAlpha;
            }
            else
            {
                newTargetAlpha = 1f;
            }
        }
        else
        {
            bool isWithinHorizontalBounds = IsPlayerWithinHorizontalBounds(playerHorizontalWorldPosition);
            
            if (isWithinHorizontalBounds)
            {
                newTargetAlpha = 0f;
            }
            else
            {
                newTargetAlpha = 1f;
            }
        }
        
        if (newTargetAlpha != _targetAlpha)
        {
            _targetAlpha = newTargetAlpha;
            
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            
            _fadeCoroutine = StartCoroutine(FadeToAlpha(_targetAlpha));
        }
    }

    private bool IsPlayerWithinHorizontalBounds(float playerHorizontalPosition)
    {
        if (_fadeOutLeftBoundary == null || _fadeOutRightBoundary == null)
        {
            return true;
        }
        
        float leftBound = _fadeOutLeftBoundary.position.x;
        float rightBound = _fadeOutRightBoundary.position.x;
        
        return playerHorizontalPosition >= leftBound && playerHorizontalPosition <= rightBound;
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        Color currentColor = _renderer.color;
        float startAlpha = currentColor.a;
        
        while (!Mathf.Approximately(_renderer.color.a, targetAlpha))
        {
            float newAlpha = Mathf.MoveTowards(_renderer.color.a, targetAlpha, Time.deltaTime * _fadeSpeed);
            currentColor.a = newAlpha;
            _renderer.color = currentColor;
            
            if (Mathf.Abs(_renderer.color.a - targetAlpha) < 0.01f)
            {
                currentColor.a = targetAlpha;
                _renderer.color = currentColor;
                break;
            }
            
            yield return null;
        }
    }
}
