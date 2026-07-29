using FunkyCode;
using UnityEngine;
using FMODUnity;

public class Torch : MonoBehaviour
{
    [SerializeField] private bool _isLit = true;
    [SerializeField] private EventReference _lightUpSfx;
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
        _lightSprite.enabled = true;
        _lightSpriteFlicker.enabled = true;
        SoundFXManager.obj.Play2D(_lightUpSfx);
    }

    public bool IsLit() {
        return _isLit;
    }
}
