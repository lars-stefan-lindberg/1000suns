using FunkyCode;
using UnityEngine;

//Default darkness lighting rgb: 545454 (in case it gets overwritten)
//new(0.33f, 0.33f, 0.33f, 1f);
public class SetDarknessTrigger : MonoBehaviour
{
    [SerializeField] private Color _darknessColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;

    private bool IsFading = false;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(LightingManager2D.Get().profile.DarknessColor != _darknessColor) {
                IsFading = true;
            }
        }
    }

    void Update() {
        if(IsFading) {
            Color darknessColor = LightingManager2D.Get().profile.DarknessColor;
            if(darknessColor != _darknessColor) {
                LightingManager2D.Get().profile.DarknessColor = Color.Lerp(darknessColor, _darknessColor, _fadeSpeed * Time.deltaTime); 
            } else {
                IsFading = false;
                LightingManager2D.Get().profile.DarknessColor = _darknessColor;
            }
        }
    }

    public void StartFade() {
        IsFading = true;
    }
}
