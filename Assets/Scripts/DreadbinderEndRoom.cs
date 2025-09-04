using System.Collections;
using UnityEngine;

public class DreadbinderEndRoom : MonoBehaviour
{
    [SerializeField]
    private Sprite _newSprite;
    [SerializeField] private GhostTrailManager _ghostTrail;
    [SerializeField]
    private float distance = 10f;
    [SerializeField]
    private float duration = 1f;

    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [ContextMenu("Change Sprite Then Move Right")]
    public void ChangeSpriteThenMoveRight()
    {
        StartCoroutine(ChangeSpriteThenMoveRight_Coroutine());
    }

    private IEnumerator ChangeSpriteThenMoveRight_Coroutine()
    {
        _spriteRenderer.sprite = _newSprite;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * distance;

        if (duration <= 0f)
        {
            transform.position = targetPos;
            yield break;
        }

        float elapsed = 0f;
        
        SoundFXManager.obj.PlayPrisonerSlide(transform);
        _ghostTrail.ShowGhosts();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t); 
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        transform.position = targetPos;
    }
}
