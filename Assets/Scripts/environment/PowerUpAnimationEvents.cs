using UnityEngine;

public class PowerUpAnimationEvents : MonoBehaviour
{
    private LightSprite2DFadeManager _lightSprite2DFadeManager;

    void Awake() {
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
    }

    public void FadeOutLight() {
        _lightSprite2DFadeManager.StartFadeOut();
    }

    public void FadeInLight() {
        _lightSprite2DFadeManager.StartFadeIn();
    }
}
