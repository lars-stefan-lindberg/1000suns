using FunkyCode;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [SerializeField] private bool _isLit = true;
    private LightSprite2D _lightSprite;
    private LightSprite2DFlicker _lightSpriteFlicker;

    void Awake()
    {
        _lightSprite = GetComponent<LightSprite2D>();
        _lightSpriteFlicker = GetComponent<LightSprite2DFlicker>();
    }

    void Start()
    {
        if(!_isLit) {
            _lightSprite.enabled = false;
            _lightSpriteFlicker.enabled = false;
        }
    }

    public void LightUp() {
        SoundFXManager.obj.PlayJump(transform);
        _lightSprite.enabled = true;
        _lightSpriteFlicker.enabled = true;
    }
}
