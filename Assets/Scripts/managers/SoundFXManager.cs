using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager obj;

    [SerializeField] private AudioSource soundFXObject;

    public AudioClip[] jump;
    public AudioClip[] land;
    public AudioClip[] step;

    void Start() {
        obj = this;
    }

    public void PlayJump(Transform spawnTransform) {
        PlayRandomSound(jump, spawnTransform, 1f);
    }

    public void PlayLand(Transform spawnTransform) {
        PlayRandomSound(land, spawnTransform, 1f);
    }

    public void PlayStep(Transform spawnTransform) {
        PlayRandomSound(step, spawnTransform, 1f);
    }

    public void PlaySound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = clip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSound(AudioClip[] clip, Transform spawnTransform, float volume)
    {
        int random = Random.Range(0, clip.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = clip[random];

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
