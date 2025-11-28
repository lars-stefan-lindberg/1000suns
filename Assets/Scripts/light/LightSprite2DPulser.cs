using UnityEngine;
using FunkyCode;

public class LightSprite2DPulser : MonoBehaviour
{
    [SerializeField] private float bpm = 60f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private AnimationCurve pulseShape = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float phaseOffsetSeconds = 0f;
    [SerializeField] private float peakHoldSeconds = 0.05f;

    LightSprite2D lightSource;

    void Start() {
        lightSource = GetComponent<LightSprite2D>();
    }

    void FixedUpdate() {
        if (lightSource == null) return;

        float freq = Mathf.Max(0.0001f, bpm / 60f);
        float period = 1f / freq; // seconds per full up-down cycle
        float hold = Mathf.Clamp(peakHoldSeconds, 0f, period * 0.9f);
        float half = Mathf.Max(0.0001f, (period - hold) * 0.5f); // rise and fall durations

        float cycleTime = (Time.time + phaseOffsetSeconds) % period;

        float normalized;
        if (cycleTime < half) {
            // Rise phase: 0 -> 1
            float u = cycleTime / half;
            float shapedRise = pulseShape != null ? pulseShape.Evaluate(u) : u;
            normalized = Mathf.Clamp01(shapedRise);
        } else if (cycleTime < half + hold) {
            // Hold at peak
            normalized = 1f;
        } else {
            // Fall phase: 1 -> 0
            float u = (cycleTime - half - hold) / half; // 0..1
            // Mirror the shape for descent for smoothness
            float shapedFall = pulseShape != null ? pulseShape.Evaluate(1f - Mathf.Clamp01(u)) : 1f - Mathf.Clamp01(u);
            normalized = Mathf.Clamp01(shapedFall);
        }

        float a = Mathf.Lerp(minAlpha, maxAlpha, normalized);
        var c = lightSource.color;
        c.a = a;
        lightSource.color = c;
    }
}
