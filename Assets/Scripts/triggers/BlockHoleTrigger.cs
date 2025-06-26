using UnityEngine;

public class BlockHoleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _floor;
    [SerializeField] private float _fadeMultiplier = 3.5f;
    private bool _fadeInSprite = false;
    private SpriteRenderer _floorRenderer;

    void Awake() {
        _floorRenderer = _floor.GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak)
            return;
        if(!GameEventManager.obj.FirstPowerUpPicked)
            return;
        if(_floor.activeSelf)
            return;
        if(other.gameObject.CompareTag("Player")) {
            _floor.SetActive(true);
            SoundFXManager.obj.PlayBrokenFloorReappear(transform);
            _fadeInSprite = true;
        }
    }

    void FixedUpdate() {
        if(_fadeInSprite && _floorRenderer.color.a < 1) {
            _floorRenderer.color = new Color(_floorRenderer.color.r, _floorRenderer.color.b, _floorRenderer.color.g, Mathf.MoveTowards(_floorRenderer.color.a, 1, _fadeMultiplier * Time.deltaTime));
        }
    }
}
