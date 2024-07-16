using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPowerUpAnimationMgr : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem.isPlaying) _particleSystem.Stop();
    }

    public void Play()
    {
        if (!_particleSystem.isPlaying) _particleSystem.Play();
    }

    public void Stop()
    {
        if (_particleSystem.isPlaying) _particleSystem.Stop();
    }

    void Update()
    {
        ParticleSystem.ShapeModule shape = _particleSystem.shape;
        shape.position = new Vector2(Player.obj.transform.position.x, Player.obj.transform.position.y);
    }
}
