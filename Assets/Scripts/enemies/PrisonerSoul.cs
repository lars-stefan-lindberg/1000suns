using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrisonerSoul : MonoBehaviour
{
    public float speed = 1.5f;

    public Transform Target {get; set;}

    void Update()
    {
        if (Target != null)
            transform.position = Vector2.MoveTowards(transform.position, Target.position, speed * Time.deltaTime);
    }
}
