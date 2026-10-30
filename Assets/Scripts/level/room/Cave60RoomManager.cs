using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Cave60RoomManager : MonoBehaviour
{
    [Header("Rumble")]
    [Tooltip("Minimum seconds between the end of one rumble and the start of the next.")]
    [SerializeField, Min(0f)] private float _rumbleMinInterval = 5f;
    [Tooltip("Maximum seconds between the end of one rumble and the start of the next.")]
    [SerializeField, Min(0f)] private float _rumbleMaxInterval = 10f;
    [Tooltip("How long the camera shakes and the earthquake sound plays before fading out.")]
    [SerializeField, Min(0f)] private float _rumbleDuration = 2f;
    [SerializeField] private float _rumbleShakeAmplitude = 1.94f;
    [SerializeField] private float _rumbleShakeFrequency = 1.84f;
    [SerializeField] private EventReference _earthquakeStinger;

    private bool _isRumbling;
    private Coroutine _rumbleCoroutine;
    private EventInstance _rumbleInstance;

    // Rumbles immediately, then at random intervals until StopRumbling is called
    public void StartRumbling() {
        _isRumbling = true;
        if (_rumbleCoroutine == null) _rumbleCoroutine = StartCoroutine(RumbleLoop());
    }

    // A rumble that's already playing finishes, but no new ones start
    public void StopRumbling() {
        _isRumbling = false;
    }

    void OnDisable() {
        if (_rumbleCoroutine != null) StopCoroutine(_rumbleCoroutine);
        _rumbleCoroutine = null;
        _isRumbling = false;
        AudioUtils.SafeStop(ref _rumbleInstance);
    }

    private IEnumerator RumbleLoop() {
        while (_isRumbling) {
            yield return Rumble(_rumbleDuration);
            float wait = Random.Range(_rumbleMinInterval, _rumbleMaxInterval);
            for (float t = 0f; t < wait && _isRumbling; t += Time.deltaTime)
                yield return null;
        }
        _rumbleCoroutine = null;
    }

    //Shakes the camera and plays earthquake sfx
    private IEnumerator Rumble(float duration) {
        CameraShakeManager.obj.ShakeCamera(_rumbleShakeAmplitude, _rumbleShakeFrequency, duration);
        _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
        _rumbleInstance.start();
        _rumbleInstance.release();
        yield return new WaitForSeconds(duration);
        AudioUtils.SafeStop(ref _rumbleInstance);
    }
}
