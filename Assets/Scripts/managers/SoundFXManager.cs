using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager obj;

    public AudioClip jump;
    public AudioClip land;
    public AudioClip step;

    private AudioSource _audioSource;

    void Start() {
        obj = this;
    }

    void Awake() {
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayJump() {
        playSound(jump);
    }

    public void PlayLand() {
        playSound(land);
    }

    public void PlayStep() {
        playSound(step);
    }

    private void playSound(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
