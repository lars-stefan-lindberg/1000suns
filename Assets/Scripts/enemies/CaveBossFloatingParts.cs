using System;
using UnityEngine;
using Random = UnityEngine.Random;

// Makes the cave boss heads and body parts float around their origin positions so the boss looks alive.
// Each part plays an endless, randomized stream of motion patterns (line, circle, figure-8, ...) that blend into each other,
// with a bit of noise on top. The offset never exceeds the configured radius. Hits spike the movement, defeat sustains it.
// Lives on the "Body parts" object; parts move in their parent's local space so the hit shake on the parent still layers on top.
public class CaveBossFloatingParts : MonoBehaviour
{
    public enum PatternType { Line, Horizontal, Vertical, Circle, Ellipse, FigureEight }

    [Serializable]
    public class PatternWeight {
        public PatternType type;
        [Tooltip("Relative chance of this pattern being picked. 0 = never.")]
        [Min(0f)] public float weight = 1f;
    }

    [Serializable]
    public class FloatingPart {
        public Transform part;
        [Tooltip("Multiplies the base radius for this part.")]
        [Min(0f)] public float distanceMultiplier = 1f;
        [Tooltip("Multiplies the movement speed for this part.")]
        [Min(0f)] public float speedMultiplier = 1f;
        [Tooltip("Scales the movement range per axis, e.g. (1, 0.5) = half as much vertical movement.")]
        public Vector2 axisScale = Vector2.one;

        [NonSerialized] public Vector3 originPosition;
        [NonSerialized] public Vector3 originScale;
        [NonSerialized] public Pattern currentPattern;
        [NonSerialized] public Pattern previousPattern;
        [NonSerialized] public float blendProgress;
        [NonSerialized] public float patternTimeLeft;
        [NonSerialized] public Vector2 noiseSeed;
        [NonSerialized] public float noiseTime;
        [NonSerialized] public float breathPhase;
        [NonSerialized] public float breathAngularSpeed;
        [NonSerialized] public bool isInitialized;
    }

    // Runtime state of one motion pattern. Evaluates to an offset within the unit circle.
    public class Pattern {
        public PatternType type;
        public float amplitude;
        public float angularSpeed;
        public float phase;
        public Vector2 direction;
        public float ellipseRatio;
        public float spinDirection;

        public void Advance(float deltaTime) {
            phase = Mathf.Repeat(phase + angularSpeed * deltaTime, TwoPi);
        }

        public Vector2 Evaluate() {
            float sin = Mathf.Sin(phase);
            float cos = Mathf.Cos(phase);
            Vector2 local;
            switch (type) {
                case PatternType.Horizontal: return new Vector2(sin * amplitude, 0f);
                case PatternType.Vertical: return new Vector2(0f, sin * amplitude);
                case PatternType.Line: return direction * (sin * amplitude);
                case PatternType.Circle: local = new Vector2(cos, sin * spinDirection); break;
                case PatternType.Ellipse: local = new Vector2(cos, sin * spinDirection * ellipseRatio); break;
                default: local = new Vector2(sin, Mathf.Sin(phase * 2f) * FigureEightHeight); break;
            }
            // Rotate the local shape so it can face any direction
            Vector2 rotated = new Vector2(local.x * direction.x - local.y * direction.y, local.x * direction.y + local.y * direction.x);
            return rotated * amplitude;
        }
    }

    private const float TwoPi = Mathf.PI * 2f;
    private const float FigureEightHeight = 0.5f;
    private const float NoiseSeedRange = 1000f;
    private const float NoiseCenter = 0.5f;
    private const float NoiseToSigned = 2f;
    private const float MinimumDuration = 0.01f;
    private const int GizmoSegments = 32;

    [Header("Parts")]
    [Tooltip("Heads and body parts that float. Leave out the main body.")]
    [SerializeField] private FloatingPart[] _parts;

    [Header("Range")]
    [Tooltip("Max distance (units) a part moves from its origin, before per-part and agitation multipliers. 8 px = 1 unit.")]
    [SerializeField, Min(0f)] private float _radius = 0.5f;
    [Tooltip("Size of each pattern as a fraction of the radius, picked randomly per pattern.")]
    [SerializeField] private Vector2 _patternAmplitudeRange = new Vector2(0.4f, 1f);

    [Header("Speed & Timing")]
    [Tooltip("Seconds for one full cycle of a pattern (back and forth, one lap, ...), picked randomly per pattern.")]
    [SerializeField] private Vector2 _cycleDurationRange = new Vector2(2f, 6f);
    [Tooltip("Seconds a pattern plays before blending into the next one, picked randomly per pattern.")]
    [SerializeField] private Vector2 _patternDurationRange = new Vector2(4f, 10f);
    [Tooltip("Seconds to blend from one pattern to the next.")]
    [SerializeField, Min(MinimumDuration)] private float _patternBlendDuration = 1.5f;

    [Header("Patterns")]
    [SerializeField] private PatternWeight[] _patternWeights = {
        new PatternWeight { type = PatternType.Line },
        new PatternWeight { type = PatternType.Horizontal },
        new PatternWeight { type = PatternType.Vertical },
        new PatternWeight { type = PatternType.Circle },
        new PatternWeight { type = PatternType.Ellipse },
        new PatternWeight { type = PatternType.FigureEight },
    };
    [Tooltip("Width-to-length ratio of ellipse patterns, picked randomly per pattern.")]
    [SerializeField] private Vector2 _ellipseRatioRange = new Vector2(0.3f, 0.7f);

    [Header("Noise")]
    [Tooltip("Random wobble added on top of the patterns, as a fraction of the radius.")]
    [SerializeField, Range(0f, 1f)] private float _noiseStrength = 0.15f;
    [Tooltip("How fast the wobble changes.")]
    [SerializeField, Min(0f)] private float _noiseFrequency = 0.3f;

    [Header("Breathing")]
    [Tooltip("Gentle scale pulse per part. Keep the strength low (1-3%) to limit pixel distortion.")]
    [SerializeField] private bool _breathingEnabled;
    [Tooltip("Max scale change as a fraction of the original scale.")]
    [SerializeField, Range(0f, 0.2f)] private float _breathingStrength = 0.02f;
    [Tooltip("Seconds per breath, picked randomly per part.")]
    [SerializeField] private Vector2 _breathingDurationRange = new Vector2(2.5f, 4f);

    [Header("Hit")]
    [Tooltip("Distance multiplier at the peak of a hit.")]
    [SerializeField, Min(0f)] private float _hitDistanceMultiplier = 1.5f;
    [Tooltip("Speed multiplier at the peak of a hit.")]
    [SerializeField, Min(0f)] private float _hitSpeedMultiplier = 2f;
    [Tooltip("Seconds for a hit spike to fade back to the normal float.")]
    [SerializeField, Min(MinimumDuration)] private float _hitFadeDuration = 2f;

    [Header("Escalation (per hit taken)")]
    [Tooltip("Added to the base speed multiplier for every hit taken, so the boss gets more desperate. 0.3 = +30% per hit.")]
    [SerializeField, Min(0f)] private float _escalationSpeedPerHit = 0.3f;
    [Tooltip("Added to the base distance multiplier for every hit taken. 0.1 = +10% per hit.")]
    [SerializeField, Min(0f)] private float _escalationDistancePerHit = 0.1f;

    [Header("Defeated")]
    [Tooltip("Distance multiplier kept from defeat until the boss is gone.")]
    [SerializeField, Min(0f)] private float _defeatedDistanceMultiplier = 1.5f;
    [Tooltip("Speed multiplier kept from defeat until the boss is gone.")]
    [SerializeField, Min(0f)] private float _defeatedSpeedMultiplier = 2f;

    [Header("Response")]
    [Tooltip("How quickly the movement reacts to hit and defeat changes. Higher = snappier.")]
    [SerializeField, Min(0f)] private float _agitationResponse = 8f;

    private float _hitWeight;
    private int _hitsTaken;
    private bool _isDefeated;
    private float _currentDistanceMultiplier = 1f;
    private float _currentSpeedMultiplier = 1f;

    void Awake() {
        foreach (var part in _parts) InitializePart(part);
    }

    void OnDisable() {
        foreach (var part in _parts) {
            if (part.part == null || !part.isInitialized) continue;
            part.part.localPosition = part.originPosition;
            part.part.localScale = part.originScale;
        }
    }

    void Update() {
        float deltaTime = Time.deltaTime;
        UpdateAgitation(deltaTime);
        foreach (var part in _parts) {
            if (part.part == null || !part.isInitialized) continue;
            UpdatePart(part, deltaTime);
        }
    }

    // Spikes the movement distance and speed, then fades back to a base float that escalates with every hit
    public void Agitate() {
        _hitWeight = 1f;
        _hitsTaken++;
    }

    // Keeps the movement at the defeated agitation level from now on
    public void SetDefeated() {
        _isDefeated = true;
    }

    private void InitializePart(FloatingPart part) {
        if (part.part == null) return;
        part.originPosition = part.part.localPosition;
        part.originScale = part.part.localScale;
        part.noiseSeed = new Vector2(Random.Range(0f, NoiseSeedRange), Random.Range(0f, NoiseSeedRange));
        part.noiseTime = 0f;
        part.breathPhase = Random.Range(0f, TwoPi);
        part.breathAngularSpeed = TwoPi / Mathf.Max(MinimumDuration, RandomInRange(_breathingDurationRange));
        // Start from the origin and blend into the first pattern
        part.previousPattern = null;
        part.currentPattern = CreateRandomPattern();
        part.blendProgress = 0f;
        part.patternTimeLeft = GetPatternDuration();
        part.isInitialized = true;
    }

    private void UpdateAgitation(float deltaTime) {
        _hitWeight = Mathf.Max(0f, _hitWeight - deltaTime / _hitFadeDuration);
        float hit = Mathf.SmoothStep(0f, 1f, _hitWeight);
        // Escalated base level the hit spike is layered on top of
        float baseDistance = 1f + _hitsTaken * _escalationDistancePerHit;
        float baseSpeed = 1f + _hitsTaken * _escalationSpeedPerHit;
        float targetDistance = baseDistance * Mathf.Lerp(1f, _hitDistanceMultiplier, hit);
        float targetSpeed = baseSpeed * Mathf.Lerp(1f, _hitSpeedMultiplier, hit);
        // Defeated values are absolute, used only when stronger than the escalated level
        if (_isDefeated) {
            targetDistance = Mathf.Max(targetDistance, _defeatedDistanceMultiplier);
            targetSpeed = Mathf.Max(targetSpeed, _defeatedSpeedMultiplier);
        }
        // Smoothed so hits don't make the parts jump
        float response = 1f - Mathf.Exp(-_agitationResponse * deltaTime);
        _currentDistanceMultiplier = Mathf.Lerp(_currentDistanceMultiplier, targetDistance, response);
        _currentSpeedMultiplier = Mathf.Lerp(_currentSpeedMultiplier, targetSpeed, response);
    }

    private void UpdatePart(FloatingPart part, float deltaTime) {
        float motionDeltaTime = deltaTime * part.speedMultiplier * _currentSpeedMultiplier;
        UpdatePatternSequence(part, deltaTime);
        part.currentPattern.Advance(motionDeltaTime);
        if (part.previousPattern != null) part.previousPattern.Advance(motionDeltaTime);

        Vector2 previous = part.previousPattern != null ? part.previousPattern.Evaluate() : Vector2.zero;
        Vector2 offset = Vector2.Lerp(previous, part.currentPattern.Evaluate(), Mathf.SmoothStep(0f, 1f, part.blendProgress));

        part.noiseTime += motionDeltaTime * _noiseFrequency;
        offset += GetNoise(part) * _noiseStrength;
        offset = Vector2.ClampMagnitude(offset, 1f);

        float distance = _radius * part.distanceMultiplier * _currentDistanceMultiplier;
        Vector2 scaledOffset = Vector2.Scale(offset, part.axisScale) * distance;
        part.part.localPosition = part.originPosition + (Vector3)scaledOffset;

        if (_breathingEnabled) {
            part.breathPhase = Mathf.Repeat(part.breathPhase + part.breathAngularSpeed * motionDeltaTime, TwoPi);
            part.part.localScale = part.originScale * (1f + Mathf.Sin(part.breathPhase) * _breathingStrength);
        }
        else {
            part.part.localScale = part.originScale;
        }
    }

    // Counts down the current pattern and blends into a new random one when it runs out
    private void UpdatePatternSequence(FloatingPart part, float deltaTime) {
        part.patternTimeLeft -= deltaTime;
        if (part.patternTimeLeft <= 0f) {
            part.previousPattern = part.currentPattern;
            part.currentPattern = CreateRandomPattern();
            part.blendProgress = 0f;
            part.patternTimeLeft = GetPatternDuration();
        }
        if (part.blendProgress < 1f) {
            part.blendProgress = Mathf.Min(1f, part.blendProgress + deltaTime / _patternBlendDuration);
            if (part.blendProgress >= 1f) part.previousPattern = null;
        }
    }

    // Never shorter than the blend, so a blend always finishes before the next one starts
    private float GetPatternDuration() {
        return Mathf.Max(RandomInRange(_patternDurationRange), _patternBlendDuration);
    }

    private Vector2 GetNoise(FloatingPart part) {
        float x = (Mathf.PerlinNoise(part.noiseSeed.x, part.noiseTime) - NoiseCenter) * NoiseToSigned;
        float y = (Mathf.PerlinNoise(part.noiseSeed.y, part.noiseTime) - NoiseCenter) * NoiseToSigned;
        return new Vector2(x, y);
    }

    private Pattern CreateRandomPattern() {
        float angle = Random.Range(0f, TwoPi);
        return new Pattern {
            type = PickPatternType(),
            amplitude = Mathf.Clamp01(RandomInRange(_patternAmplitudeRange)),
            angularSpeed = TwoPi / Mathf.Max(MinimumDuration, RandomInRange(_cycleDurationRange)),
            phase = Random.Range(0f, TwoPi),
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)),
            ellipseRatio = RandomInRange(_ellipseRatioRange),
            spinDirection = Random.value < 0.5f ? -1f : 1f,
        };
    }

    // Weighted random pick. Falls back to a circle if no pattern has weight.
    private PatternType PickPatternType() {
        float totalWeight = 0f;
        foreach (var entry in _patternWeights) totalWeight += entry.weight;
        if (totalWeight <= 0f) return PatternType.Circle;

        float pick = Random.Range(0f, totalWeight);
        foreach (var entry in _patternWeights) {
            if (pick < entry.weight) return entry.type;
            pick -= entry.weight;
        }
        return _patternWeights[_patternWeights.Length - 1].type;
    }

    private static float RandomInRange(Vector2 range) {
        return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
    }

    // Shows each part's normal movement range (cyan) and its max agitated range (red)
    void OnDrawGizmosSelected() {
        if (_parts == null) return;
        float maxAgitation = Mathf.Max(1f, _hitDistanceMultiplier, _defeatedDistanceMultiplier);
        Gizmos.matrix = transform.localToWorldMatrix;
        foreach (var part in _parts) {
            if (part.part == null) continue;
            Vector3 center = Application.isPlaying && part.isInitialized ? part.originPosition : part.part.localPosition;
            Vector2 extents = part.axisScale * (_radius * part.distanceMultiplier);
            Gizmos.color = Color.cyan;
            DrawEllipse(center, extents);
            Gizmos.color = Color.red;
            DrawEllipse(center, extents * maxAgitation);
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    private static void DrawEllipse(Vector3 center, Vector2 extents) {
        Vector3 previous = center + new Vector3(extents.x, 0f, 0f);
        for (int i = 1; i <= GizmoSegments; i++) {
            float angle = TwoPi * i / GizmoSegments;
            Vector3 next = center + new Vector3(Mathf.Cos(angle) * extents.x, Mathf.Sin(angle) * extents.y, 0f);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
