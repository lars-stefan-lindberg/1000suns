using System.Collections;
using FunkyCode;
using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    public static PlayerLightManager obj;
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 5f;

    void Awake() {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }

    void Update()
    {
        Transform activeTransform = PlayerManager.obj.GetPlayerTransform();
        if (activeTransform == null) return;

        _light.transform.position = activeTransform.position;
    }

    public void FadeOut() {
        StartCoroutine(FadeOutLight());
    }

    private IEnumerator FadeOutLight() {
        float timeElapsed = 0;
        Light2D light = _light.GetComponent<Light2D>();
        Color startColor = light.color;
        while (timeElapsed < 2f) {
            float t = timeElapsed / 2f;
            t = t * t * (3f - 2f * t);
            light.color = Color.Lerp(startColor, Color.clear, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        light.color = Color.clear;
        yield return null;
    }
}
