using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EliSaveFileCardPortrait : MonoBehaviour, ISaveFileCardPortrait
{
    [SerializeField] private GameObject _cape;
    [SerializeField] private Animator _eyesAnimator;
    [SerializeField] private Animator _braidAnimator;
    
    private Image[] _portraitImages;
    private Material[] _instancedMaterials;

    void Awake()
    {
        InitializeMaterials();
    }

    void Start() {
        //Layer index 0 = idle
        _eyesAnimator.SetLayerWeight(0, 1);
        // Start with animations paused
        _eyesAnimator.speed = 0;
        _braidAnimator.speed = 0;
    }

    public void SetCape(bool hasCape) {
        if(hasCape)
            _cape.SetActive(true);
        else
            _cape.SetActive(false);
    }

    public void StartAnimation() {
        // Set speed to 1 first, then play to ensure animation starts
        _braidAnimator.speed = 1;
        _eyesAnimator.speed = 1;
        
        // Play the idle animation to ensure it's actually running
        AnimatorStateInfo currentState = _eyesAnimator.GetCurrentAnimatorStateInfo(0);
        float normalizedTime = currentState.normalizedTime;
        _eyesAnimator.Play("idle", 0, normalizedTime);
    }

    public void StopAnimation() {
        // Reset to idle state and pause
        _eyesAnimator.Play("idle", 0, 0);
        _braidAnimator.speed = 0;
        _eyesAnimator.speed = 0;
    }

    public void ResetAndStopAnimation() {
        _eyesAnimator.Play("idle", 0, 0);
        _braidAnimator.Play("sway", 0, 0);
        _braidAnimator.speed = 0;
        _eyesAnimator.speed = 0;
    }

    private void InitializeMaterials()
    {
        _portraitImages = GetComponentsInChildren<Image>();
        _instancedMaterials = new Material[_portraitImages.Length];
        
        for (int i = 0; i < _portraitImages.Length; i++)
        {
            if (_portraitImages[i].material != null)
            {
                _instancedMaterials[i] = Instantiate(_portraitImages[i].material);
                _portraitImages[i].material = _instancedMaterials[i];
            }
        }
    }

    [ContextMenu("Blink")]
    public void Blink()
    {
        _eyesAnimator.SetTrigger("blink");
    }

    [ContextMenu("DoubleBlink")]
    public void DoubleBlink()
    {
        _eyesAnimator.SetTrigger("doubleBlink");
    }
    
    public Animator GetEyesAnimator() {
        return _eyesAnimator;
    }
}
