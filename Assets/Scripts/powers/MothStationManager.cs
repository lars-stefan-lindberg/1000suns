using System.Collections;
using FunkyCode;
using UnityEngine;

public class MothStationManager : MonoBehaviour
{
    [SerializeField] private Animator _mothAnimator;
    [SerializeField] private LightSprite2D _torch;
    [SerializeField] private LightSprite2DFlicker _torchFlicker;
    private BoxCollider2D _collider;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && !PlayerPowersManager.obj.CanForcePushJump && PlayerManager.obj.GetActivePlayerType() == PlayerManager.PlayerType.HUMAN)
        {
            StartCoroutine(Activated());
        }
    }

    private IEnumerator Activated() {
        SoundFXManager.obj.PlayPlayerPickupCavePowerup(transform);
        PlayerPowersManager.obj.CanForcePushJump = true;
        Player.obj.FlashOnce();
        MothsManager.obj.SpawnMoths();
        StartCoroutine(FlashTorch());

        _collider.enabled = false;
        _mothAnimator.speed = 2f;

        yield return new WaitForSeconds(2f);
        _collider.enabled = true;
        _mothAnimator.speed = 1f;
    }

    private IEnumerator FlashTorch() {
        _torchFlicker.enabled = false;

        // Cache originals
        Color originalColor = _torch.color;
        Vector3 originalScale = _torch.lightSpriteTransform.scale;

        // Targets: RGB to white, keep original alpha; scale to 3x on X/Y
        Color targetColor = new Color(1f, 1f, 1f, originalColor.a);
        Vector3 targetScale = new Vector3(2.5f, 2.5f, originalScale.z);

        // Phase 1: quick ramp up
        float upDuration = 0.15f;
        float t = 0f;
        while (t < upDuration) {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / upDuration);
            _torch.color = Color.Lerp(originalColor, targetColor, lerp);
            _torch.lightSpriteTransform.scale = Vector3.Lerp(originalScale, targetScale, lerp);
            yield return null;
        }
        _torch.color = targetColor;
        _torch.lightSpriteTransform.scale = targetScale;

        // Phase 2: brief hold at max
        yield return new WaitForSeconds(0.1f);

        // Phase 3: slower return
        float downDuration = 0.2f;
        t = 0f;
        while (t < downDuration) {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / downDuration);
            _torch.color = Color.Lerp(targetColor, originalColor, lerp);
            _torch.lightSpriteTransform.scale = Vector3.Lerp(targetScale, originalScale, lerp);
            yield return null;
        }
        _torch.color = originalColor;
        _torch.lightSpriteTransform.scale = originalScale;

        _torchFlicker.enabled = true;
    }
}

