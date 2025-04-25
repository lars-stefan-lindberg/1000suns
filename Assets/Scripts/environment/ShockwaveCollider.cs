using System.Collections;
using UnityEngine;

public class ShockwaveCollider : MonoBehaviour
{
    public float force = 50f;
    public float radius = 10f;
    public float duration = 0.5f;

    private void Start()
    {
        // Optionally expand the shockwave visually
        transform.localScale = Vector3.zero;
        StartCoroutine(ExpandShockwave());
    }

    private IEnumerator ExpandShockwave()
    {
        float time = 0;
        Vector3 targetScale = Vector3.one * radius * 2f; // *2 because scale is diameter
        Vector3 startScale = transform.localScale;

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        Destroy(gameObject); // Destroy after expansion
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            // Use the force value as the hit boost
            PlayerBlobMovement.obj.OnHit(-1f, force);
        }
    }
}
