using UnityEngine;
using System;

public class ChildCollider : MonoBehaviour
{
    public event Action<Collision2D> OnCollisionEnterEvent;
    public event Action<Collision2D> OnCollisionStayEvent;
    public event Action<Collision2D> OnCollisionExitEvent;

    void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        OnCollisionStayEvent?.Invoke(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        OnCollisionExitEvent?.Invoke(collision);
    }
}
