using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
    public static UISoundPlayer obj;

    [SerializeField] private UISoundLibrary sounds;

    void Awake() {
        obj = this;
    }

    public void PlaySelect() {
        SoundFXManager.obj.Play2D(sounds.select);
    }

    public void PlayPlayGame() {
        SoundFXManager.obj.Play2D(sounds.playGame);
    }

    public void PlayBack() {
        SoundFXManager.obj.Play2D(sounds.back);
    }

    public void PlaySliderTick() {
        SoundFXManager.obj.Play2D(sounds.sliderTick);
    }

    public void PlayBrowse() {
        SoundFXManager.obj.Play2D(sounds.browse);
    }

    void OnDestroy() {
        obj = null;
    }
}
