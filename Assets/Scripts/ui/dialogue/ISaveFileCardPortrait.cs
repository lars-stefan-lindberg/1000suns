using UnityEngine;

public interface ISaveFileCardPortrait
{
    void StartAnimation();
    void StopAnimation();
    void ResetAndStopAnimation();
    void Blink();
    void DoubleBlink();
    Animator GetEyesAnimator();
}
