using UnityEngine;

public class PrisonerSoul : MonoBehaviour
{
    public float speed = 1.5f;

    public Transform Target {get; set;}
    public bool IsTargetReached {get; private set;}

    void Update()
    {
        if (Target != null) {
            transform.position = Vector2.MoveTowards(transform.position, Target.position, speed * Time.deltaTime);
            if (transform.position == Target.position) {
                IsTargetReached = true;
            }
        }
    }
}
