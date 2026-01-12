using FunkyCode;
using FunkyCode.Utilities;
using UnityEngine;

public class LightSprite2DOverlayPulser : MonoBehaviour
{
    [SerializeField] private float flickersPerSecond = 15f;
    [SerializeField] private float flickerRangeMin = -0.1f;
    [SerializeField] private float flickerRangeMax = 0.1f;

    LightSprite2D lightSource;
    float lightAlpha;
    TimerHelper timer;

    void Start() {
        lightSource = GetComponent<LightSprite2D>();
        lightAlpha = lightSource.meshMode.alpha;
        
        timer = TimerHelper.Create();
    }

    void Update() {
        if (timer == null) {
            timer = TimerHelper.Create();
            return;
        }

        if (timer.GetMillisecs() > 1000f / flickersPerSecond) {
            float tempAlpha = lightAlpha;
            tempAlpha += Random.Range(flickerRangeMin, flickerRangeMax);
            lightSource.meshMode.alpha = tempAlpha;
            timer.Reset();
        }
    }
}
