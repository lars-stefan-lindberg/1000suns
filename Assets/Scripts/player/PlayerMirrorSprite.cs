using UnityEngine;

public class PlayerMirrorSprite : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Vector2 playerOffset = new Vector2(1.3f, 0.12f);
    [SerializeField] private Transform _anchor;
    void Start()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        //Update position
        Vector2 playerPosition = Player.obj.transform.position;
        transform.position = new Vector2(playerPosition.x + playerOffset.x, playerPosition.y + playerOffset.y);

        _renderer.sprite = PlayerMovement.obj.spriteRenderer.sprite;
        _renderer.flipX = PlayerMovement.obj.spriteRenderer.flipX;
        _anchor.localScale = PlayerMovement.obj.spriteRenderer.gameObject.transform.parent.transform.localScale;
    }
}
