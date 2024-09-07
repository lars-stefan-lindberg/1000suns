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
            CollectibleManager.Instance.CollectiblePickedTemporary(id);
            Destroy(gameObject);
        }
    }

    void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(CollectibleManager.Instance.IsCollectiblePicked(id)) {
            _spriteRenderer.enabled = false;
            Destroy(gameObject);
        }
    }
}
