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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            roomObjects.ForEach(roomObject => roomObject.SetActive(true));
            Color darknessColor = LightingManager2D.Get().profile.DarknessColor;
            if(darknessColor != _darknessColor) {
                _fadeDarknessCoroutine = StartCoroutine(FadeDarkness(darknessColor));
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

    private IEnumerator FadeDarkness(Color startColor) {
        Debug.Log("Changing darkness level.");
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

        Debug.Log("Darkness level: " + _darknessColor);
        LightingManager2D.Get().profile.DarknessColor = _darknessColor;
    }
}
