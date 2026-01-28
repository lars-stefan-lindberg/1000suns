using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave4SlabTrigger : MonoBehaviour
{
    [SerializeField] private GameEventId _isCutsceneCompleted;
    [SerializeField] private SpriteFlash _spriteFlash;
    [SerializeField] private List<StoneFloat> _stones;
    [SerializeField] private float _stoneStartFloatDelay = 0.2f;
    [SerializeField] private float _stoneFloatingTime = 3f;
    [SerializeField] private float _coolDownTime = 10f;
    private float _nextAllowedTriggerTime;

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
        //TODO: Sound effect for slab reacting
        _spriteFlash.Flash();
        yield return new WaitForSeconds(_stoneStartFloatDelay);
        foreach (var stone in _stones) {
            stone.TriggerFloat();
        }
        yield return new WaitForSeconds(_stoneFloatingTime);
        foreach (var stone in _stones) {
            stone.TriggerFall();
        }

        yield return null;
    }

    public void Reset()
    {
        _spriteFlash.AbortFlash();
        foreach (var stone in _stones) {
            stone.Reset();
        }
    }
}
