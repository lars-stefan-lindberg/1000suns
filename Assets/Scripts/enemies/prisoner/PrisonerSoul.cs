using UnityEngine;

public class PrisonerSoul : MonoBehaviour
{
    public float speed = 1.5f;

    public Vector3 Target {get; set;}
    public bool IsTargetReached {get; private set;}

    void Update()
    {
        if (Target != null) {
            transform.position = Vector2.MoveTowards(transform.position, Target, speed * Time.deltaTime);
            if (transform.position == Target) {
                IsTargetReached = true;
            }
        }
    }
}
