using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPortalSprite : MonoBehaviour
{
    // Min and max values for y-scale
    public float minYScale = 0.8f;
    public float maxYScale = 1.2f;
    // Min and max values for duration
    public float minScaleDuration = 0.5f;
    public float maxScaleDuration = 2.0f;

    private Coroutine scaleCoroutine;

    void Start()
    {
        if(PlayerPowersManager.obj.CanTurnFromBlobToHuman) {
            Destroy(gameObject.transform.parent.gameObject);
            return;
        }
        scaleCoroutine = StartCoroutine(ScaleYLoop());
    }

    IEnumerator ScaleYLoop()
    {
        float currentY = minYScale;
        while (true)
        {
            float targetY = (Mathf.Approximately(currentY, minYScale)) ? maxYScale : minYScale;
            float duration = Random.Range(minScaleDuration, maxScaleDuration);
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = new Vector3(startScale.x, targetY, startScale.z);
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScale;
            currentY = targetY;
        }
    }
}
