public interface IPortrait
{
    void SwitchEmotion(string emotion);
    void Blink();
    void DoubleBlink();
    void PlayVFX(PortraitVFX vfx);
    bool IsLeft { get; set; }
}
