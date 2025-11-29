using UnityEngine;

public class WaterfallScroller : MonoBehaviour
{
    public Transform sprite1;
    public Transform sprite2;

    public float scrollSpeed = 2f;

    private float spriteHeight;
    private float bottomStartBoundaryY;
    private SpriteRenderer sr1;
    private SpriteRenderer sr2;

    void Start()
    {
        sr1 = sprite1.GetComponent<SpriteRenderer>();
        sr2 = sprite2.GetComponent<SpriteRenderer>();
        // Assumes both sprites are the same height
        spriteHeight = sr1.bounds.size.y;

        // Capture the initial bottom boundary as the lower sprite's bottom edge (bounds.min.y)
        bool sprite1IsLower = sr1.bounds.center.y < sr2.bounds.center.y;
        bottomStartBoundaryY = sprite1IsLower ? sr1.bounds.min.y : sr2.bounds.min.y;
    }

    void Update()
    {
        float delta = scrollSpeed * Time.deltaTime;

        // Determine which sprite is currently on top and which is at the bottom
        bool sprite1IsTop = sr1.bounds.center.y >= sr2.bounds.center.y;
        Transform top = sprite1IsTop ? sprite1 : sprite2;
        Transform bottom = sprite1IsTop ? sprite2 : sprite1;
        SpriteRenderer topSR = sprite1IsTop ? sr1 : sr2;

        // Predict crossing this frame using pre-move and post-move bottom edge (bounds.min.y)
        float topMinY_pre = topSR.bounds.min.y;
        float topMinY_post = topMinY_pre - delta;
        bool willCrossThisFrame = topMinY_pre > bottomStartBoundaryY && topMinY_post <= bottomStartBoundaryY;

        // If crossed, reposition the current bottom sprite above the current top BEFORE moving
        if (willCrossThisFrame)
        {
            bottom.position = new Vector3(
                bottom.position.x,
                top.position.y + spriteHeight,
                bottom.position.z
            );
        }

        // Move both sprites downward (world space)
        sprite1.Translate(Vector3.down * delta, Space.World);
        sprite2.Translate(Vector3.down * delta, Space.World);
    }
}
