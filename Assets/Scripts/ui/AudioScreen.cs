using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioScreen : UIScreen
{
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private TMP_Text _musicSliderValue;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _sfxSliderValue;

    public UnityEvent OnBack;

    protected override void OnBeforeShow()
    {
        _musicSlider.value = AudioOptions.obj.MusicStep;
        _sfxSlider.value = AudioOptions.obj.SfxStep;
    }

    public void MusicLevelChanged(float volume) {
        UISoundPlayer.obj.PlaySliderTick();
        AudioOptions.obj.SetMusicStep(volume);
        _musicSliderValue.text = ((int) volume).ToString();
    }

    public void SfxLevelChanged(float volume) {
        UISoundPlayer.obj.PlaySliderTick();
        AudioOptions.obj.SetSfxStep(volume);
        _sfxSliderValue.text = ((int) volume).ToString();
    }

    public void ResetToDefaults() {
        AudioOptions.obj.SetMusicStep(10);
        AudioOptions.obj.SetSfxStep(10);
        _musicSlider.value = 10f;
        _sfxSlider.value = 10f;
    }

    public void Back() {
        UISoundPlayer.obj.PlayBack();
        OnBack?.Invoke();
    }
}
