using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlobCharge : MonoBehaviour
{
public static PlayerBlobCharge obj;

    private BoxCollider2D _collider;

    public float minBuildUpPowerTime = 0.3f;

    public float maxForce = 3;
    public float powerUpMaxForce = 4;
    public float powerBuildUpPerFixedUpdate = 1.2f;

    public float defaultPower = 1;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    
    private AudioSource _forcePushStartChargingAudioSource;
    private AudioSource _forcePushChargeLoopAudioSource;
    private bool _startedForcePushChargeLoop = false;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(!PlayerPowersManager.obj.BlobCanExtraJump) {
            return;
        }
        if (context.performed)
        {
            if (defaultPower < StaminaMgr.obj.GetCurrentStamina()) {
                _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
                _buildUpPower = defaultPower;
                _buildingUpPower = true;
                _buildUpPowerTime = 0;
                PlayerBlob.obj.StartChargeFlash();
            }
        }
        if(context.canceled) {
            if(PlayerBlobMovement.obj.IsTransitioningBetweenLevels()) {
                ResetBuiltUpPower();
                return;
            }
            //Need to check that we are building power before we can push. If not the push will be executed on button release.
            if(_buildingUpPower && _buildUpPowerTime >= minBuildUpPowerTime)
            {
                ForcePush();
            }
            
            ResetBuiltUpPower();
        }
    }

    public bool IsFullyCharged() {
        return _buildUpPower >= maxForce;
    }

    public void ResetBuiltUpPower() {
        if(_forcePushStartChargingAudioSource != null && _forcePushStartChargingAudioSource.isPlaying) {
            SoundFXManager.obj.FadeOutAndStopSound(_forcePushStartChargingAudioSource, 0.05f);
            _forcePushStartChargingAudioSource = null;
        }
        if(_forcePushChargeLoopAudioSource != null && _forcePushChargeLoopAudioSource.isPlaying) {
            SoundFXManager.obj.FadeOutAndStopSound(_forcePushChargeLoopAudioSource, 0.05f);
            _startedForcePushChargeLoop = false;
            _forcePushChargeLoopAudioSource = null;
        }

        _buildingUpPower = false;
        _buildUpPower = defaultPower;
        _buildUpPowerTime = 0;
        PlayerBlob.obj.EndChargeFlash();
    }

    private void FixedUpdate()
    {
        if(_buildingUpPower) {
            _buildUpPowerTime += Time.deltaTime;
            if(_buildUpPower < maxForce && _buildUpPowerTime > minBuildUpPowerTime) {
                _buildUpPower *= powerBuildUpPerFixedUpdate;
            }
        }

        if(IsFullyCharged() && !_startedForcePushChargeLoop) {
            _startedForcePushChargeLoop = true;
            _forcePushChargeLoopAudioSource = SoundFXManager.obj.PlayForcePushChargeLoop(transform);
        }
    }

    public float projectileDelay = 0.1f;

    void ForcePush() {
        Push();
    }

    public float testPower = 10;
    void Push()
    {
        ExecuteForcePushVfx();

        SoundFXManager.obj.PlayForcePushExecute(transform);
        //PlayerBlob.obj.rigidBody.AddForce(new Vector2(0, testPower));
        PlayerBlobMovement.obj.ExecuteChargedJump();
    }

    public void ExecuteForcePushVfx() {
        ShockWaveManager.obj.CallShockWave(_collider.bounds.center);
        //TODO
        //Player.obj.ForcePushFlash();
        CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.transform.DOShakePosition(0.13f, new Vector3(0.15f, 0.15f, 0), 30, 90, false, true, ShakeRandomnessMode.Harmonic);
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
