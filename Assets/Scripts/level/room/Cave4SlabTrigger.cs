using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Cave4SlabTrigger : MonoBehaviour
{
    [SerializeField] private GameEventId _isCutsceneCompleted;
    [SerializeField] private SpriteFlash _spriteFlash;
    [SerializeField] private List<StoneFloat> _stones;
    [SerializeField] private EventReference _stonesStart;
    [SerializeField] private EventReference _stonesImpact;
    [SerializeField] private float _stoneStartFloatDelay = 0.2f;
    [SerializeField] private float _stoneFloatingTime = 3f;
    [SerializeField] private float _waitBeforeStoneImpact = 0.3f;
    [SerializeField] private float _coolDownTime = 10f;
    private float _nextAllowedTriggerTime;
    private EventInstance _stonesStartInstance;
    private EventInstance _stonesImpactInstance;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!GameManager.obj.HasEvent(_isCutsceneCompleted))
            return;
        if (Time.time < _nextAllowedTriggerTime)
            return;
        if(!collision.CompareTag("Player"))
            return;

        _nextAllowedTriggerTime = Time.time + _coolDownTime;
        StartCoroutine(StartVfx());
    }

    public IEnumerator StartVfx() {
        _spriteFlash.Flash();
        yield return new WaitForSeconds(_stoneStartFloatDelay);
        _stonesStartInstance = SoundFXManager.obj.CreateAttachedInstance(_stonesStart, gameObject, null);
        _stonesStartInstance.start();
        _stonesStartInstance.release();
        foreach (var stone in _stones) {
            stone.TriggerFloat();
        }
        yield return new WaitForSeconds(_stoneFloatingTime);
        foreach (var stone in _stones) {
            stone.TriggerFall();
        }
        yield return new WaitForSeconds(_waitBeforeStoneImpact);
        _stonesImpactInstance = SoundFXManager.obj.CreateAttachedInstance(_stonesImpact, gameObject, null);
        _stonesImpactInstance.start();
        _stonesImpactInstance.release();

        yield return null;
    }

    public EventInstance GetStonesStartInstance() => _stonesStartInstance;
    public EventInstance GetStonesImpactInstance() => _stonesImpactInstance;

    public void Reset()
    {
        AudioUtils.SafeStop(ref _stonesStartInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _stonesImpactInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        _spriteFlash.AbortFlash();
        foreach (var stone in _stones) {
            stone.Reset();
        }
    }
}
