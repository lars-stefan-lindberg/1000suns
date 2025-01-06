using System.Collections;
using UnityEngine;

public class TwoMushrooms : MonoBehaviour
{
    private Animator _animator;
    private BoxCollider2D _collider;
    [SerializeField] private Transform _anchorTransform;
    private bool _playerEntered;

    void Awake() {
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponentInChildren<BoxCollider2D>();
    }

    private float _collisionMargin = 0.5f;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player") && !_playerEntered) {
            Bounds playerCollisionBounds = other.bounds;
            Bounds mushRoomBounds = _collider.bounds;
            Vector2 playerBottom = new(playerCollisionBounds.center.x, playerCollisionBounds.center.y - playerCollisionBounds.extents.y);
            Vector2 mushroomTop = new(mushRoomBounds.center.x, mushRoomBounds.center.y + mushRoomBounds.extents.y); 
            bool landedOnMushroom = playerBottom.y > mushroomTop.y - _collisionMargin;

            _animator.SetTrigger("wiggle");
            _playerEntered = true;

            if(landedOnMushroom)
                StartCoroutine(Squeeze(_squeezeX, _squeezeY, _squeezeTime));
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            _playerEntered = false;
        }
    }

    private float _squeezeX = 1.25f;
    private float _squeezeY = 0.65f;
    private float _squeezeTime = 0.1f;
    private IEnumerator Squeeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float time = 0f;
        while (time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            _anchorTransform.localScale = Vector3.Lerp(originalSize, newSize, time);
            yield return null;
        }
        time = 0f;
        while(time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            _anchorTransform.localScale = Vector3.Lerp(newSize, originalSize, time);
            yield return null;
        }
    }
}
