using UnityEngine;

/*
How the mirror works:
- The mirror consists of three sprites
    - The frame. Put in background sorting layer.
    - Mirror glass (transparent). Same as above. This is used to make the lighting look decent. Put in default sorting layer, -5.
    - Mirror glass. A Sprite Mask component is added, the same sprite is added to the mask as the main sprite. The sprite is put in default sorting layer, -8.
- A copy of the player object is created, and uses only this script to copy the main players sprite into correct position.
    - The sprite of the mirrored player uses the value "Visible Inside Mask" for "Mask interaction". The sorting layer is set to default, -6.
        This enables the sprite to only be visible inside the mirror glass mask.
- The same setup is done for the cave avatar as well.
*/
public class PlayerMirrorSprite : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Vector2 playerOffset = new Vector2(1.5f, 0.25f);
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
