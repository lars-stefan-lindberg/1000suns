using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural tentacle with tiered sprite thickness.
/// A FABRIK-style chain is simulated with a few joints; sprites ("stamps")
/// are then placed at even spacing along the curve, using a thicker sprite
/// near the base and thinner ones toward the tip.
/// Setup: put this on an empty child of the boss body, fill in Tiers
/// (base first, tip last), and call SetTarget() from your boss script.
/// </summary>
public class CaveBossTentacleChain : MonoBehaviour
{
    [System.Serializable]
    public class Tier
    {
        public GameObject prefab;
        [Tooltip("Tier is used until this fraction of the tentacle length (0 = base, 1 = tip). Last tier should be 1.")]
        [Range(0f, 1f)] public float endsAt = 1f;
        [Tooltip("Distance between stamps in world units. Roughly the sprite's diameter, a bit less to overlap.")]
        public float spacing = 0.2f;
    }

    [Header("Tiers (base -> tip)")]
    public Tier[] tiers;

    [Header("Chain")]
    [Tooltip("When on, the tentacle always reaches the target: joint count and joint length are derived from the distance to the target.")]
    public bool autoLength = true;
    [Tooltip("Auto length: preferred length of each link. Smaller = more joints, smoother bends.")]
    public float preferredJointLength = 0.5f;
    public int minJoints = 3;
    public int maxJoints = 64;
    [Tooltip("Fixed length only (Auto Length off).")]
    public int jointCount = 8;
    [Tooltip("Fixed length only (Auto Length off).")]
    public float jointLength = 0.5f;
    public float tipSpeed = 12f;
    public int solverIterations = 2;

    [Header("Wiggle (visual only)")]
    public float wiggleAmplitude = 0.25f;
    public float wiggleFrequency = 2.5f;
    public float wigglePhasePerJoint = 0.6f;

    [Header("Pixel look")]
    public bool snapToPixels = true;
    public float pixelsPerUnit = 16f;

    [Header("Hierarchy")]
    [Tooltip("Parent for spawned stamps. Leave empty to use this object. Should have scale (1,1,1) and no flip/rotation.")]
    public Transform stampParent;

    Vector2[] pos;          // simulated joints
    Vector2[] draw;         // joints with wiggle applied
    float[] lens;           // length of each drawn piece
    Vector2 targetPoint;
    Vector2 tipPoint;
    List<Transform>[] pools;
    int[] used;
    float segLen;           // current length of each link
    float wigglePhase;      // random per tentacle so copies don't move in sync

    float MaxReach => jointLength * (jointCount - 1);

    void Start()
    {
        int n = Mathf.Max(jointCount, 2);
        pos = new Vector2[n];
        draw = new Vector2[n];
        lens = new float[n - 1];

        Vector2 root = transform.position;
        for (int i = 0; i < n; i++)
            pos[i] = root + Vector2.down * jointLength * i;

        tipPoint = pos[n - 1];
        wigglePhase = Random.value * Mathf.PI * 2f;
        targetPoint = tipPoint;

        pools = new List<Transform>[tiers.Length];
        used = new int[tiers.Length];
        for (int i = 0; i < tiers.Length; i++) pools[i] = new List<Transform>();
    }

    public void SetTarget(Vector2 worldPoint) => targetPoint = worldPoint;

    void LateUpdate()
    {
        Vector2 root = transform.position;
        float follow = 1f - Mathf.Exp(-tipSpeed * Time.deltaTime);

        if (autoLength)
        {
            // Length follows the smoothed tip, so the tentacle grows/shrinks smoothly
            tipPoint = Vector2.Lerp(tipPoint, targetPoint, follow);
            float length = Vector2.Distance(root, tipPoint);
            int n = Mathf.Clamp(Mathf.RoundToInt(length / Mathf.Max(preferredJointLength, 0.01f)) + 1,
                                Mathf.Max(minJoints, 2), Mathf.Max(maxJoints, minJoints, 2));
            if (n != pos.Length) Resize(n);
            segLen = length / (n - 1);
        }
        else
        {
            if (pos.Length != Mathf.Max(jointCount, 2)) Resize(Mathf.Max(jointCount, 2));
            segLen = jointLength;

            Vector2 toTarget = targetPoint - root;
            if (toTarget.magnitude > MaxReach)
                toTarget = toTarget.normalized * MaxReach;
            tipPoint = Vector2.Lerp(tipPoint, root + toTarget, follow);
        }

        Solve(root);
        ApplyWiggle();
        PlaceStamps();
    }

    // Resample the current chain shape to a new joint count, keeping its shape
    void Resize(int n)
    {
        int oldN = pos.Length;
        float[] cum = new float[oldN];
        for (int i = 1; i < oldN; i++)
            cum[i] = cum[i - 1] + Vector2.Distance(pos[i - 1], pos[i]);
        float total = cum[oldN - 1];

        var newPos = new Vector2[n];
        int seg = 0;
        for (int i = 0; i < n; i++)
        {
            float d = total * i / (n - 1);
            while (seg < oldN - 2 && cum[seg + 1] < d) seg++;
            float segL = cum[seg + 1] - cum[seg];
            float u = segL > 0.0001f ? (d - cum[seg]) / segL : 0f;
            newPos[i] = Vector2.Lerp(pos[seg], pos[seg + 1], u);
        }

        pos = newPos;
        draw = new Vector2[n];
        lens = new float[n - 1];
    }

    void Solve(Vector2 root)
    {
        int last = pos.Length - 1;
        for (int iter = 0; iter < solverIterations; iter++)
        {
            pos[last] = tipPoint;
            for (int i = last - 1; i >= 0; i--)
                pos[i] = pos[i + 1] + SafeDir(pos[i] - pos[i + 1]) * segLen;

            pos[0] = root;
            for (int i = 1; i <= last; i++)
                pos[i] = pos[i - 1] + SafeDir(pos[i] - pos[i - 1]) * segLen;
        }
    }

    void ApplyWiggle()
    {
        int last = pos.Length - 1;
        for (int i = 0; i < pos.Length; i++)
        {
            Vector2 a = pos[Mathf.Max(i - 1, 0)];
            Vector2 b = pos[Mathf.Min(i + 1, last)];
            Vector2 dir = SafeDir(b - a);
            Vector2 perp = new Vector2(-dir.y, dir.x);

            float t = i / (float)last;
            float wave = Mathf.Sin(Time.time * wiggleFrequency - i * wigglePhasePerJoint + wigglePhase);
            draw[i] = pos[i] + perp * wave * wiggleAmplitude * t;
        }
    }

    void PlaceStamps()
    {
        for (int i = 0; i < used.Length; i++) used[i] = 0;

        float total = 0f;
        for (int i = 0; i < lens.Length; i++)
        {
            lens[i] = Vector2.Distance(draw[i], draw[i + 1]);
            total += lens[i];
        }

        if (total > 0.0001f)
        {
            float d = 0f, segStart = 0f;
            int seg = 0;

            while (d < total)
            {
                while (seg < lens.Length - 1 && segStart + lens[seg] < d)
                {
                    segStart += lens[seg];
                    seg++;
                }

                float u = lens[seg] > 0.0001f ? (d - segStart) / lens[seg] : 0f;
                Vector2 p = Vector2.Lerp(draw[seg], draw[seg + 1], u);

                int tier = TierFor(d / total);
                Stamp(tier, p);
                d += Mathf.Max(tiers[tier].spacing, 0.01f);
            }

            // Always finish with a stamp exactly at the tip
            Stamp(tiers.Length - 1, draw[draw.Length - 1]);
        }

        // Hide stamps we didn't use this frame
        for (int t = 0; t < pools.Length; t++)
            for (int i = used[t]; i < pools[t].Count; i++)
                if (pools[t][i].gameObject.activeSelf) pools[t][i].gameObject.SetActive(false);
    }

    int TierFor(float t)
    {
        for (int i = 0; i < tiers.Length - 1; i++)
            if (t <= tiers[i].endsAt) return i;
        return tiers.Length - 1;
    }

    void Stamp(int tier, Vector2 p)
    {
        if (snapToPixels)
            p = new Vector2(Mathf.Round(p.x * pixelsPerUnit), Mathf.Round(p.y * pixelsPerUnit)) / pixelsPerUnit;

        var pool = pools[tier];
        if (used[tier] >= pool.Count)
            pool.Add(Instantiate(tiers[tier].prefab, stampParent != null ? stampParent : transform).transform);

        var tr = pool[used[tier]++];
        if (!tr.gameObject.activeSelf) tr.gameObject.SetActive(true);
        tr.position = p;
    }

    static Vector2 SafeDir(Vector2 v)
    {
        return v.sqrMagnitude > 1e-6f ? v.normalized : Vector2.down;
    }

    void OnDestroy()
    {
        if (pools == null) return;
        foreach (var pool in pools)
            foreach (var tr in pool)
                if (tr != null) Destroy(tr.gameObject);
    }
}