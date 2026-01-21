using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FunkyCode;
using System.Collections;

//Standard room size: X: 40, Y: 22.5
public class RoomMgr : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomObjects;
    [SerializeField] private Color _darknessColor;
    public UnityEvent OnRoomEnter;
    public UnityEvent OnRoomExit;
    private float _darknessFadeSpeed = 9f;
    private Coroutine _fadeDarknessCoroutine;
    private float _roomTransitionDelay = 0.2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            roomObjects.ForEach(roomObject => roomObject.SetActive(true));
            Color darknessColor = LightingManager2D.Get().profile.DarknessColor;
            if(darknessColor != _darknessColor) {
                if(IsBrighterThan(_darknessColor, darknessColor)) {
                    _fadeDarknessCoroutine = StartCoroutine(FadeDarkness(darknessColor, _roomTransitionDelay));
                } else {
                    _fadeDarknessCoroutine = StartCoroutine(FadeDarkness(darknessColor, 0f));
                }
            }
            OnRoomEnter?.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if(_fadeDarknessCoroutine != null) {
                StopCoroutine(_fadeDarknessCoroutine);
            }
            roomObjects.ForEach(roomObject => roomObject.SetActive(false));
            OnRoomExit?.Invoke();
        }
    }

    private IEnumerator FadeDarkness(Color startColor, float delay) {
        yield return new WaitForSeconds(delay); //If changing to brighter, it looks better if we wait for room transition to complete

        if (startColor == _darknessColor)
        {
            LightingManager2D.Get().profile.DarknessColor = _darknessColor;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Clamp01(t + (_darknessFadeSpeed * Time.deltaTime));
            LightingManager2D.Get().profile.DarknessColor = Color.Lerp(startColor, _darknessColor, t);
            yield return null;
        }

        LightingManager2D.Get().profile.DarknessColor = _darknessColor;
    }

    private static bool IsBrighterThan(Color a, Color b)
    {
        return GetLuminance(a) > GetLuminance(b);
    }

    private static float GetLuminance(Color c)
    {
        // Standard Rec. 709 luminance formula
        return 0.2126f * c.r +
            0.7152f * c.g +
            0.0722f * c.b;
    }
}
