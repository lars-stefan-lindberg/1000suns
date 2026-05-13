using UnityEngine;

public class HoveringDebris : MonoBehaviour
{
    [Header("Hover")]
    public float hoverSpeed = 2f;
    public float hoverAmplitude = 0.05f;
    public float hoverPauseDuration = 0.15f;
    public float hoverStartOffset = 0f;

    private float hoverBaseY;
    private float hoverTimer;
    private float hoverPhaseOffset;
    private bool isHovering;

    private enum State
    {
        HoverPause,
        Hovering
    }

    private State currentState;

    void Start()
    {
        hoverBaseY = transform.position.y;
        hoverTimer = hoverPauseDuration;
        hoverPhaseOffset = Time.time - hoverStartOffset;
        currentState = State.HoverPause;
        isHovering = true;
    }

    void Update()
    {
        if (!isHovering)
            return;

        HandleHover();
    }

    void HandleHover()
    {
        switch (currentState)
        {
            case State.HoverPause:
                hoverTimer -= Time.deltaTime;
                if (hoverTimer <= 0f)
                    currentState = State.Hovering;
                break;

            case State.Hovering:
                float t = (Time.time - hoverPhaseOffset) * hoverSpeed;
                float hover = Mathf.Sin(t) * hoverAmplitude;

                transform.position = new Vector3(
                    transform.position.x,
                    hoverBaseY + hover,
                    transform.position.z
                );
                break;
        }
    }
}
