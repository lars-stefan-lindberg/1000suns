using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
    public static PlayerPush obj;

    public int playerOffset = 1;
    public float maxForce = 40;
    public float powerBuildUpPerFixedUpdate = 1;

    public float defaultPower = 20;
    public float pushTiltPower = 2000;
    public float fallTiltPower = 10000;

    [Header("Dependecies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    public float minBuildUpPowerTime = 0.5f;
    private SpriteRenderer _playerSpriteRenderer;
    public float dashStopMultiplier = 0.4f;
    public float horizontalDashSpeed = 1f;

    public FloatyPlatform platform;

    private bool CanBuildPower =>
        _buildingUpPower &&
        _buildUpPower < maxForce &&
        _buildUpPower <= StaminaMgr.obj.GetCurrentStamina();


    private void Awake()
    {
        obj = this;
        _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (defaultPower < StaminaMgr.obj.GetCurrentStamina()) {
                _buildUpPower = defaultPower;
                _buildingUpPower = true;
                _buildUpPowerTime = 0;
            }
        }
        if(context.canceled) {
            //Need to check that we are building power before we can push. If not the push will be executed on button release.
            if(_buildingUpPower && _buildUpPowerTime >= minBuildUpPowerTime)
            {
                StaminaMgr.Push push = new();
                push.SetEffort(_buildUpPower); //TODO: This is just luck for now, that power equals effort
                StaminaMgr.obj.ExecutePower(push);

                //Should tilt the player slightly backwards in air
                if(!PlayerMovement.obj.isGrounded && !PlayerMovement.obj.isFalling)
                {
                    //Tilt player slightly when in air
                    float power = PlayerMovement.obj.isFacingLeft() ? pushTiltPower : -pushTiltPower;
                    Player.obj.rigidBody.AddForce(new Vector2(power, 0));
                } else if(PlayerMovement.obj.isFalling)
                    PlayerMovement.obj.ExecuteFallDash();

                if(platform != null)
                    platform.MovePlatform();

                if(PlayerMovement.obj.isGrounded && !PlayerMovement.obj.isFalling && Player.obj.hasPowerUp) 
                    PlayerMovement.obj.ExecuteForcePushJump();
                Push(_buildUpPower);
            }
            
            _buildUpPower = defaultPower;
            _buildingUpPower = false;
        }
    }

    private void FixedUpdate()
    {
        if (CanBuildPower)
        {
            _buildUpPowerTime += Time.deltaTime;
            //_buildUpPower = defaultPower * (Mathf.Round(_buildUpPowerTime) + 2);
            _buildUpPower = defaultPower; //Only one power available for now
        }
        if (_buildingUpPower)
        {
            pushPowerUpAnimation.GetComponent<PushPowerUpAnimationMgr>().Play();
        }
        else
        {
            pushPowerUpAnimation.GetComponent<PushPowerUpAnimationMgr>().Stop();
        }
    }

    void Push(float power)
    {
        int playerFacingDirection = _playerSpriteRenderer.flipX ? -1 : 1;

        ProjectileManager.obj.shootProjectile(
            new Vector3(gameObject.transform.position.x + (playerOffset * playerFacingDirection) , gameObject.transform.position.y, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        Player.obj.hasPowerUp = false;
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
