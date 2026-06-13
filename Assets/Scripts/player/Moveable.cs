using UnityEngine;

public class Moveable : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody;

    public Rigidbody2D GetRigidbody() {
        return _rigidbody;
    }
}
