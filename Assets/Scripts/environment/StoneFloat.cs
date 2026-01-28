using UnityEngine;
using UnityEngine.Events;

public class StoneFloat : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.5f;
    public float riseSpeed = 1.5f;

    [Header("Gravity Fall")]
    public float gravity = 25f;
    public float maxFallSpeed = 8f;

    [Header("Height Randomization")]
    public float floatHeightVariance = 0.15f;

    [Header("Hover")]
    public float hoverSpeed = 2f;
    public float hoverAmplitude = 0.05f;
    public float hoverPauseDuration = 0.15f;

    [Header("Events")]
    public UnityEvent onFloatStart;
    public UnityEvent onLand;

    private float randomizedFloatHeight;
    private float verticalVelocity;
    private Vector3 groundPosition;
    private State currentState;

    private float hoverBaseY;
    private float hoverTimer;
    private float hoverPhaseOffset;

    private enum State
    {
        Grounded,
        Rising,
        HoverPause,
        Floating,
        Falling
    }

    void Awake()
    {
        groundPosition = transform.position;
        currentState = State.Grounded;
    }

    void Start()
    {
        groundPosition = transform.position;
        currentState = State.Grounded;

        randomizedFloatHeight = floatHeight + Random.Range(
            -floatHeightVariance,
            floatHeightVariance
        );
    }

    public void Reset()
    {
        transform.position = groundPosition;
        verticalVelocity = 0f;
        hoverTimer = 0f;
        currentState = State.Grounded;
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 floatTarget = groundPosition + Vector3.up * randomizedFloatHeight;

        switch (currentState)
        {
            case State.Rising:
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    floatTarget,
                    riseSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, floatTarget) < 0.001f)
                {
                    transform.position = floatTarget;
                    hoverBaseY = transform.position.y;
                    hoverTimer = hoverPauseDuration;
                    hoverPhaseOffset = Time.time;
                    currentState = State.HoverPause;
                }
                break;

            case State.HoverPause:
                hoverTimer -= Time.deltaTime;
                if (hoverTimer <= 0f)
                    currentState = State.Floating;
                break;

            case State.Floating:
                float t = (Time.time - hoverPhaseOffset) * hoverSpeed;
                float hover = Mathf.Sin(t) * hoverAmplitude;

                transform.position = new Vector3(
                    transform.position.x,
                    hoverBaseY + hover,
                    transform.position.z
                );
                break;

            case State.Falling:
                // Apply gravity
                verticalVelocity -= gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);

                // Move vertically
                transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

                // Clamp to ground
                if (transform.position.y <= groundPosition.y)
                {
                    transform.position = groundPosition;
                    verticalVelocity = 0f;
                    currentState = State.Grounded;
                    onLand?.Invoke();
                }
                break;
        }
    }

    // =========================
    // PUBLIC TRIGGERS
    // =========================

    public void TriggerFloat()
    {
        if (currentState == State.Rising ||
            currentState == State.HoverPause ||
            currentState == State.Floating)
            return;

        currentState = State.Rising;
        verticalVelocity = 0f;
        onFloatStart?.Invoke();
    }

    public void TriggerFall()
    {
        if (currentState == State.Falling ||
            currentState == State.Grounded)
            return;

        currentState = State.Falling;
        verticalVelocity = 0f;
    }

    // Optional helpers
    public bool IsFloating =>
        currentState == State.Rising ||
        currentState == State.HoverPause ||
        currentState == State.Floating;

    public bool IsGrounded =>
        currentState == State.Grounded;
}
