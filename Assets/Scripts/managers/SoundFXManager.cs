using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager obj;

    [SerializeField] private AudioSource soundFXObject;

    public AudioClip[] jump;
    public AudioClip[] landOnRoots;
    public AudioClip[] stepOnRoots;
    public AudioClip[] landOnRock;
    public AudioClip[] stepOnRock;

    void Start() {
        obj = this;
    }

    public void PlayJump(Transform spawnTransform) {
        PlayRandomSound(jump, spawnTransform, 1f);
    }

    public void PlayLand(Surface surface, Transform spawnTransform) {
        if(surface == Surface.Roots)
            PlayRandomSound(landOnRoots, spawnTransform, 1f);
        else if(surface == Surface.Rock)
            PlayRandomSound(landOnRock, spawnTransform, 1f);
    }

    public void PlayStep(Surface surface, Transform spawnTransform) {
        if(surface == Surface.Roots)
            PlayRandomSound(stepOnRoots, spawnTransform, 1f);
        else if(surface == Surface.Rock)
            PlayRandomSound(stepOnRock, spawnTransform, 1f);
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
