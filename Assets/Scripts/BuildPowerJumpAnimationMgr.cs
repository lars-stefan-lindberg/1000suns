using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPowerJumpAnimationMgr : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    private const float playerOffset = 0.82f;

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

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.ShapeModule shape = _particleSystem.shape;
        shape.position = new Vector2(Player.obj.transform.position.x, Player.obj.transform.position.y - playerOffset);
    }
}
