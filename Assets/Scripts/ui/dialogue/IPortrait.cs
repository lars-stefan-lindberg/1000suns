using UnityEngine;

public interface IPortrait
{
    void SwitchEmotion(string emotion);
    void SwitchFaceExpression(string expression);
    void SwitchEyesExpression(string expression);
    void Blink();
    void DoubleBlink();
    void PlayVFX(PortraitVFX vfx);
    bool IsLeft { get; set; }
    Animator GetEyesAnimator();
}
