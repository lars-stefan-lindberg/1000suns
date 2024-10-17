using System.Collections;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager obj;

    [SerializeField] private AudioSource soundFXObject;

    public AudioClip[] jump;
    public AudioClip[] landOnRoots;
    public AudioClip[] stepOnRoots;
    public AudioClip[] landOnRock;
    public AudioClip[] stepOnRock;
    public AudioClip[] blockSliding;

    public AudioClip[] breakableWallCrackling;
    public AudioClip breakableWallBreak;

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

    public void PlayStep(Surface surface, Transform spawnTransform, float volume) {
        if(surface == Surface.Roots)
            PlayRandomSound(stepOnRoots, spawnTransform, volume);
        else if(surface == Surface.Rock)
            PlayRandomSound(stepOnRock, spawnTransform, volume);
    }

    public AudioSource PlayBlockSliding(Transform spawnTransform, float soundDurationPercentage) {
        return PlaySound(blockSliding[0], spawnTransform, 1f, soundDurationPercentage);
    }

    public void PlayBreakableWallCrackling(Transform spawnTransform) {
        PlayRandomSound(breakableWallCrackling, spawnTransform, 1f);
    }

    public void PlayBreakableWallBreak(Transform spawnTransform) {
        PlaySound(breakableWallBreak, spawnTransform, 1f);
    }

    public void PlaySound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
    }

    public AudioSource PlaySound(AudioClip clip, Transform spawnTransform, float volume, float clipLengthPercentage)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        float clipLength = audioSource.clip.length * clipLengthPercentage;
        PlaySound(audioSource, clipLength);
        return audioSource;
    }

    public void PlayRandomSound(AudioClip[] clip, Transform spawnTransform, float volume)
    {
        int random = Random.Range(0, clip.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip[random];
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
    }

    public void PlaySound(AudioSource audioSource, float clipLength) {
        audioSource.Play();

        Destroy(audioSource.gameObject, clipLength);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
