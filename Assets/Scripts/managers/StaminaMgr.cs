using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaMgr : MonoBehaviour
{
    public static StaminaMgr obj;

    public float maxStamina = 100;
    public float staminaBuildUpMultiplier = 30;
    public float timeBeforeStaminaBuildUpStart = 1f;
    private float _timeAfterPowerExecuted = 1;

    private float _currentStamina = 100;

    private bool CanBuildUpStamina =>
        _timeAfterPowerExecuted >= timeBeforeStaminaBuildUpStart &&
        _currentStamina <= maxStamina;

    public bool HasEnoughStamina(IPower power)
    {
        //return power.GetEffort() <= _currentStamina;
        return true;
    }

    public float GetCurrentStamina()
    {
        return _currentStamina;
    }

    public void ExecutePower(IPower power)
    {
        //_currentStamina -= power.GetEffort();
        _timeAfterPowerExecuted = 0;
    }

    private void FixedUpdate()
    {
        if(CanBuildUpStamina) {
            _currentStamina += Time.fixedDeltaTime * staminaBuildUpMultiplier;
        }
        else
        {
            _timeAfterPowerExecuted += Time.deltaTime;
        }

        //Debug.Log("current stamina after fixed update: " + _currentStamina);
        float currentStaminaRounded = Mathf.Floor(_currentStamina);
        if (currentStaminaRounded < 0) currentStaminaRounded = 0;
        if (currentStaminaRounded > maxStamina) currentStaminaRounded = maxStamina;
    }

    private void Awake()
    {
        obj = this;
    }

    private void OnDestroy()
    {
        obj = null;
    }

    public class AirJump : IPower
    {
        private const float EFFORT = 10;

        public float GetEffort()
        {
            return EFFORT;
        }
    }

    public class PowerJump : IPower
    {
        private const float EFFORT = 100;

        public float GetEffort()
        {
            return EFFORT;
        }
    }

    public class Push : IPower
    {
        private float effort = 0;

        public void SetEffort(float effort)
        {
            this.effort = effort;
        }

        public float GetEffort()
        {
            return effort;
        }
    }
}

public interface IPower
{
    public float GetEffort();
}