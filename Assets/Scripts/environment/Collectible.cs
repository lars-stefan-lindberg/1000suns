using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private string id;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            SoundFXManager.obj.PlayCollectiblePickup(transform);
            CollectibleManager.obj.CollectiblePickedTemporarily(id);
            _spriteRenderer.enabled = false;
            Destroy(gameObject, 2);
        }
    }

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(CollectibleManager.obj.IsCollectiblePicked(id)) {
            _spriteRenderer.enabled = false;
            Destroy(gameObject);
        }
    }
}
