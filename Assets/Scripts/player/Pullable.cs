using UnityEngine;

public class Pullable : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private float _pullForce = 1f;
    [SerializeField] private bool _isHeavy = false;
    public bool IsPulled {get; set;}

    private void Awake() {
        IsPulled = false;
    }

    public Rigidbody2D GetRigidbody() {
        return _rigidBody;
    }

    public float GetPullForce() {
        return _pullForce;
    }

    public bool IsHeavy() {
        return _isHeavy;
    }
}
