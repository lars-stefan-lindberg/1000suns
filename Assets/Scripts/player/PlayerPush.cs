using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
    public static PlayerPush obj;

    private Animator _animator;
    private SpriteRenderer _playerSpriteRenderer;

    public float minBuildUpPowerTime = 0.5f;

    public int playerOffset = 1;
    public float maxForce = 3;
    public float powerUpMaxForce = 4;
    public float powerBuildUpPerFixedUpdate = 1.1f;

    public float defaultPower = 1;
    public float pushTiltPower = 2000;
    public float fallTiltPower = 10000;

    [Header("Dependecies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    
    public FloatyPlatform platform;

    bool CanUseForcePushJump => PlayerMovement.obj.isGrounded && Player.obj.hasPowerUp && _buildUpPower >= maxForce;

    private void Awake()
    {
        obj = this;
        _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
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
                //Should tilt the player slightly backwards in air
                if(!PlayerMovement.obj.isGrounded && !PlayerMovement.obj.isFalling)
                {
                    //Tilt player slightly when in air
                    float power = PlayerMovement.obj.isFacingLeft() ? pushTiltPower : -pushTiltPower;
                    Player.obj.rigidBody.AddForce(new Vector2(power, 0));
                } else if(PlayerMovement.obj.isFalling)
                    PlayerMovement.obj.ExecuteFallDash();

                if(platform != null) {
                    float power = Player.obj.hasPowerUp ? powerUpMaxForce : _buildUpPower;
                    StartCoroutine(DelayedMovePlatform(projectileDelay, power));
                }

                if(CanUseForcePushJump) 
                    ForcePushJump(powerUpMaxForce);
                else
                    ForcePush(_buildUpPower);
            }
            
            _buildUpPower = defaultPower;
            _buildingUpPower = false;
        }
    }

    private void FixedUpdate()
    {
        _buildUpPowerTime += Time.deltaTime;
        Debug.Log("build up power: " + _buildUpPower);
        if(_buildUpPower < maxForce && _buildUpPowerTime > minBuildUpPowerTime)
            _buildUpPower *= powerBuildUpPerFixedUpdate;
        if (_buildingUpPower)
        {
            pushPowerUpAnimation.GetComponent<PushPowerUpAnimationMgr>().Play();
        }
        else
        {
            pushPowerUpAnimation.GetComponent<PushPowerUpAnimationMgr>().Stop();
        }
    }

    public float projectileDelay = 0.1f;

    void ForcePushJump(float power) {
        Push(power, true);
    }

    void ForcePush(float power) {
        Push(power, false);
    }

    void Push(float power, bool forcePushJump)
    {
        _animator.SetTrigger("forcePush");
        StartCoroutine(DelayedProjectile(projectileDelay, power, forcePushJump));
    }

    private IEnumerator DelayedMovePlatform(float delay, float power) {
        yield return new WaitForSeconds(delay);
        platform.MovePlatform(PlayerMovement.obj.isFacingLeft(), power);
    }

    private IEnumerator DelayedProjectile(float delay, float power, bool forcePushJump) {
        yield return new WaitForSeconds(delay);
        int playerFacingDirection = _playerSpriteRenderer.flipX ? -1 : 1;
        ProjectileManager.obj.shootProjectile(
            new Vector3(gameObject.transform.position.x + (playerOffset * playerFacingDirection) , gameObject.transform.position.y, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        if(forcePushJump) {
           PlayerMovement.obj.ExecuteForcePushJump();
           Player.obj.hasPowerUp = false;
        }
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
