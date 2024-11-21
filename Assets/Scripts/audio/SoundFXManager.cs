using System.Collections;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager obj;

    [SerializeField] private AudioSource soundFXObject;

    #region Player
    public AudioClip[] jump;
    public AudioClip[] landOnRoots;
    public AudioClip[] stepOnRoots;
    public AudioClip[] landOnRock;
    public AudioClip[] stepOnRock;
    #endregion

    public AudioClip[] blockSliding;

    public AudioClip[] breakableWallCrackling;
    public AudioClip breakableWallBreak;

    #region UI
    public AudioClip uiBrowse;
    public AudioClip uiBack;
    public AudioClip uiConfirm;
    public AudioClip uiPlay;
    #endregion

    #region BabyPrisoner
    public AudioClip[] babyPrisonerCrawl;
    public AudioClip babyPrisonerAlert;
    public AudioClip babyPrisonerDespawn;
    public AudioClip babyPrisonerEscape;
    public AudioClip babyPrisonerScared;
    #endregion

    #region Prisoner
    public AudioClip[] prisonerCrawl;
    public AudioClip prisonerSpawn;
    public AudioClip prisonerHit;
    public AudioClip prisonerSlide;
    #endregion

    void Start() {
        obj = this;
    }

    public void PlayUIBrowse() {
        PlaySound(uiBrowse, Camera.main.transform, 1f);
    }
    public void PlayUIBack() {
        PlaySound(uiBack, Camera.main.transform, 1f);
    }
    public void PlayUIConfirm() {
        PlaySound(uiConfirm, Camera.main.transform, 1f);
    }
    public void PlayUIPlay() {
        PlaySound(uiPlay, Camera.main.transform, 1f);
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

    public void PlayBabyPrisonerCrawl(Transform spawnTransform) {
        PlayRandomSound(babyPrisonerCrawl, spawnTransform, 1f);
    }
    public void PlayBabyPrisonerAlert(Transform spawnTransform) {
        PlaySound(babyPrisonerAlert, spawnTransform, 1f);
    }
    public void PlayBabyPrisonerDespawn(Transform spawnTransform) {
        PlaySound(babyPrisonerDespawn, spawnTransform, 1f);
    }
    public void PlayBabyPrisonerScared(Transform spawnTransform) {
        PlaySound(babyPrisonerScared, spawnTransform, 1f);
    }
    public AudioSource PlayBabyPrisonerEscape(Transform spawnTransform) {
        return PlayLoopedSound(babyPrisonerEscape, spawnTransform, 1f);
    }

    public void PlayPrisonerCrawl(Transform spawnTransform) {
        PlayRandomSound(prisonerCrawl, spawnTransform, 1f);
    }
    public void PlayPrisonerSpawn(Transform spawnTransform) {
        PlaySound(prisonerSpawn, spawnTransform, 1f);
    }
    public AudioSource PlayPrisonerHit(Transform spawnTransform) {
        return PlayLoopedSound(prisonerHit, spawnTransform, 1f);
    }
    public void PlayPrisonerSlide(Transform spawnTransform) {
        PlaySound(prisonerSlide, spawnTransform, 1f);
    }

    public void PlaySound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
    }

    public AudioSource PlayLoopedSound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.parent = spawnTransform;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
    }

    public void FadeOutAndStopLoopedSound(AudioSource audioSource, float duration) {
        StartCoroutine(FadeOutAndStopLoopedSoundCoroutine(audioSource, duration));
    }

    private IEnumerator FadeOutAndStopLoopedSoundCoroutine(AudioSource audioSource, float duration) {
        float currentTime = 0;
        float currentVol = audioSource.volume;
        float targetValue = 0.0001f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioSource.volume = newVol;
            yield return null;
        }

        audioSource.Stop();
        Destroy(audioSource.gameObject);

        yield break;
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
