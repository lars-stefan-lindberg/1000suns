using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
    public int playerOffset = 1;
    public float maxForce = 40;
    public float powerBuildUpPerFixedUpdate = 1;

    public float defaultPower = 10;
    public float pushTiltPower = 2000;

    [Header("Dependecies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    private SpriteRenderer _playerSpriteRenderer;

    private bool CanBuildPower =>
        _buildingUpPower &&
        _buildUpPower < maxForce &&
        _buildUpPower <= StaminaMgr.obj.GetCurrentStamina();


    private void Awake()
    {
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
            if(_buildingUpPower)
            {
                StaminaMgr.Push push = new();
                push.SetEffort(_buildUpPower); //TODO: This is just luck for now, that power equals effort
                StaminaMgr.obj.ExecutePower(push);

                if(!PlayerMovement.obj.isGrounded && !PlayerMovement.obj.isFalling)
                {
                    float power = PlayerMovement.obj.isFacingLeft() ? pushTiltPower : -pushTiltPower;
                    Player.obj.rigidBody.AddForce(new Vector2(power, 0));
                }


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
            _buildUpPower = defaultPower * (Mathf.Round(_buildUpPowerTime) + 2);
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
        //int playerFacingDirection = transform.localScale.x > 0 ? 1 : -1;
        int playerFacingDirection = _playerSpriteRenderer.flipX ? -1 : 1;


        ProjectileManager.obj.shootProjectile(
            new Vector3(gameObject.transform.position.x + (playerOffset * playerFacingDirection) , gameObject.transform.position.y, gameObject.transform.position.z),
            playerFacingDirection,
            power);
    }


}
