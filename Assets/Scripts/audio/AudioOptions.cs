using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioOptions : MonoBehaviour
{
    public static AudioOptions obj;

    private VCA musicVCA;
    private VCA gameplaySfxVCA;
    private VCA uiSfxVCA;

    private float musicStep = 10;
    private float sfxStep = 10;

    void Awake()
    {
        obj = this;

        musicVCA       = RuntimeManager.GetVCA("vca:/music");
        gameplaySfxVCA = RuntimeManager.GetVCA("vca:/gameplay_sfx");
        uiSfxVCA       = RuntimeManager.GetVCA("vca:/ui_sfx");

        Load();
        ApplyVolumes();
    }

    // --- Public API ---

    public float MusicStep => musicStep;
    public float SfxStep => sfxStep;

    public void SetMusicStep(float step)
    {
        musicVCA.setVolume(StepToVolume(step));
        musicStep = step;
        
        Save();
    }

    public void SetSfxStep(float step)
    {
        float volume = StepToVolume(step);
        gameplaySfxVCA.setVolume(volume);
        uiSfxVCA.setVolume(volume);
        sfxStep = step;

        Save();
    }

    // --- Internal ---

    void ApplyVolumes()
    {
        SetMusicStep(musicStep);
        SetSfxStep(sfxStep);
    }

    float StepToVolume(float step)
    {
        if (step <= 0)
            return 0f;

        float normalized = Mathf.Lerp(0.05f, 1f, step / 10f);
        return Mathf.Pow(normalized, 2f);
    }

    void Load()
    {
        musicStep = PlayerPrefs.GetFloat("Audio.Music", 10);
        sfxStep   = PlayerPrefs.GetFloat("Audio.SFX", 10);
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("Audio.Music", musicStep);
        PlayerPrefs.SetFloat("Audio.SFX", sfxStep);
        PlayerPrefs.Save();
    }

    void OnDestroy() {
        obj = null;
    }
}
