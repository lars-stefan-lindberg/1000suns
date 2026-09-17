using FunkyCode;
using UnityEngine;

//Default darkness lighting rgb: 545454 (in case it gets overwritten)
//new(0.33f, 0.33f, 0.33f, 1f);
public class SetDarknessTrigger : MonoBehaviour
{
    [SerializeField] private Color _darknessColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    [SerializeField] private float _colorThreshold = 0.01f; // How close colors need to be to stop lerping
    [SerializeField] private float _maxFadeTime = 2f; // Maximum time allowed for fade (safety net)

    private bool IsFading = false;
    private float _fadeStartTime;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Color currentColor = LightingManager2D.Get().profile.DarknessColor;
            if(!ColorsAreClose(currentColor, _darknessColor)) {
                IsFading = true;
                _fadeStartTime = Time.time;
            }
        }
    }

    void Update() {
        if(IsFading) {
            Color darknessColor = LightingManager2D.Get().profile.DarknessColor;
            float fadeElapsedTime = Time.time - _fadeStartTime;
            
            // Check if we're close enough or exceeded max time
            if(ColorsAreClose(darknessColor, _darknessColor) || fadeElapsedTime >= _maxFadeTime) {
                IsFading = false;
                LightingManager2D.Get().profile.DarknessColor = _darknessColor;
            } else {
                LightingManager2D.Get().profile.DarknessColor = Color.Lerp(darknessColor, _darknessColor, _fadeSpeed * Time.deltaTime); 
            }
        }
    }

    public void StartFade() {
        IsFading = true;
        _fadeStartTime = Time.time;
    }

    private bool ColorsAreClose(Color a, Color b) {
        // Calculate the squared distance between colors (faster than using magnitude)
        float rDiff = a.r - b.r;
        float gDiff = a.g - b.g;
        float bDiff = a.b - b.b;
        float aDiff = a.a - b.a;
        float sqrDistance = rDiff * rDiff + gDiff * gDiff + bDiff * bDiff + aDiff * aDiff;
        return sqrDistance < _colorThreshold * _colorThreshold;
    }
}
