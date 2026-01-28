using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager obj;

    [SerializeField] private AudioSource spatiallyAwareSoundFXObject;
    [SerializeField] private AudioSource nonSpatiallyAwareSoundFXObject;

    #region Player
    public AudioClip[] jump;
    public AudioClip[] landOnRoots;
    public AudioClip[] stepOnRoots;
    public AudioClip[] landOnRock;
    public AudioClip[] stepOnRock;
    public AudioClip[] forcePushJump;
    public AudioClip[] forcePushLand;
    public AudioClip forcePushStartCharging;
    public AudioClip forcePushChargeLoop;
    public AudioClip forcePushExecute;
    public AudioClip genericDeath;
    public AudioClip shadowDeath;
    public AudioClip shadowSpawn;
    public AudioClip landHeavy;
    public AudioClip longFall;
    public AudioClip teleportStart;
    public AudioClip teleportEnd;
    public AudioClip[] pickupCavePowerup;
    public AudioClip[] shapeshiftToBlob;
    public AudioClip[] shapeshiftToHuman;
    #endregion

    #region Block
    public AudioClip blockSliding;
    public AudioClip blockLand;
    public AudioClip[] blockSlideOffEdge;
    public AudioClip[] blockWallImpact;
    #endregion

    #region MonsterDoor
    public AudioClip[] monsterDoorDestroy;
    public AudioClip prisonerSoulAbsorb1;
    public AudioClip prisonerSoulAbsorb2;
    public AudioClip prisonerSoulAbsorb3;
    #endregion

    #region FallingSpike
    public AudioClip[] fallingSpikeCrackling;
    public AudioClip[] fallingSpikeFall;
    public AudioClip[] fallingSpikeHit;
    public AudioClip[] fallingSpikeHitLiquid;
    #endregion

    #region Mushrooms
    public AudioClip[] mushroomBigBounce;
    public AudioClip[] mushroomSmallBounce;
    public AudioClip[] mushroomSmallRattle;
    #endregion

    public AudioClip[] breakableWallCrackling;
    public AudioClip breakableWallBreak;
    public AudioClip breakableWallHint;

    #region UI
    public AudioClip uiBrowse;
    public AudioClip uiBack;
    public AudioClip uiConfirm;
    public AudioClip uiPlay;
    public AudioClip uiSlider;

    #endregion

    #region Dialogue
    public AudioClip dialogueOpen;
    public AudioClip dialogueClose;
    public AudioClip[] dialogueConfirm;
    #endregion

    #region BabyPrisoner
    public AudioClip[] babyPrisonerCrawl;
    public AudioClip babyPrisonerAlert;
    public AudioClip babyPrisonerDespawn;
    public AudioClip babyPrisonerEscape;
    public AudioClip babyPrisonerScared;
    public AudioClip[] babyPrisonerIdle;
    #endregion

    #region Prisoner
    public AudioClip[] prisonerCrawl;
    public AudioClip[] prisonerSpawn;
    public AudioClip prisonerHit;
    public AudioClip prisonerSlide;
    public AudioClip[] prisonerDeath;
    #endregion

    #region CapeRoom
    public AudioClip capeIntroduction;
    public AudioClip capePickUp;
    #endregion

    public AudioClip revealSecret;
    public AudioClip collectiblePickup;
    public AudioClip floatingPlatformWallHit;
    public AudioClip brokenFloorReappear;
    public AudioClip brokenFloorDisappear;
    public AudioClip breakableFloorHint;
    public AudioClip caveSpaceRoomTeleport;
    public AudioClip caveAvatarEvilEyesTransition;
    public AudioClip powerUpDialogueStinger;
    public AudioClip startTransformingToBlobFirstTime;
    public AudioClip caveAvatarAttack;
    public AudioClip playerStatueShockWave;
    public AudioClip crystalRoomRumble;
    public AudioClip earthquake;
    public AudioClip c26Rumble;

    private Dictionary<AudioClip[], int> lastPickedIndices = new();
    private Dictionary<string, float> lastPlayedTimes = new();
    private const float DEFAULT_COOLDOWN = 0.1f; // Default cooldown of 0.1 seconds

    void Start() {
        obj = this;
    }

    private bool CanPlaySound(string audioId, float cooldown = DEFAULT_COOLDOWN) {
        if (!lastPlayedTimes.ContainsKey(audioId)) {
            lastPlayedTimes[audioId] = Time.time; // Allow first play
            return true;
        } else {
            float lastPlayedTime = lastPlayedTimes[audioId];
            if(lastPlayedTime < Time.time - cooldown) {
                lastPlayedTimes[audioId] = Time.time;
                return true;
            } else {
                return false;
            }
        }
    }

    public void PlayCaveAvatarAttack(Transform spawnTransform) {
        PlaySound(caveAvatarAttack, spawnTransform, 1f);
    }
    public void PlayPlayerStatueShockWave(Transform spawnTransform) {
        PlaySound(playerStatueShockWave, spawnTransform, 1f);
    }

    public void PlayBrokenFloorReappear(Transform spawnTransform) {
        PlaySound(brokenFloorReappear, spawnTransform, 1f);
    }
    public void PlayBrokenFloorDisappear(Transform spawnTransform) {
        PlaySound(brokenFloorDisappear, spawnTransform, 1f);
    }

    public void PlayBreakableFloorHint(Transform spawnTransform) {
        PlaySound(breakableFloorHint, spawnTransform, 1f);
    }

    public void PlayCollectiblePickup(Transform spawnTransform) {
        PlaySound(collectiblePickup, spawnTransform, 1f);
    }

    public void PlayFloatingPlatformWallHit(Transform spawnTransform) {
        Debug.Log("Playing floating platform wall hit sound...");
        //PlaySound(floatingPlatformWallHit, spawnTransform, 1f);
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
    public void PlayUISlider()
    {
        PlaySound(uiSlider, Camera.main.transform, 1f);
    }

    public void PlayDialogueOpen() {
        PlayNonSpatiallyAwareSound(dialogueOpen, Camera.main.transform, 1f);
    }
    public void PlayDialogueClose() {
        PlayNonSpatiallyAwareSound(dialogueClose, Camera.main.transform, 1f);
    }
    public void PlayDialogueConfirm() {
        PlayRandomNonSpatiallyAwareSound(dialogueConfirm, Camera.main.transform, 1f);
    }

    public void PlayCaveAvatarEvilEyesTransition() {
        PlayNonSpatiallyAwareSound(caveAvatarEvilEyesTransition, Camera.main.transform, 1f);
    }

    public void PlayCaveSpaceRoomTeleport() {
        PlayNonSpatiallyAwareSound(caveSpaceRoomTeleport, Camera.main.transform, 1f);
    }

    public void PlayJump(Transform spawnTransform) {
        PlayRandomSound(jump, spawnTransform, 1f);
    }

    public void PlayPowerUpDialogueStinger() {
        PlayNonSpatiallyAwareSound(powerUpDialogueStinger, Camera.main.transform, 1f);
    }

    public void PlayCrystalRoomRumble() {
        PlayNonSpatiallyAwareSound(crystalRoomRumble, Camera.main.transform, 1f);
    }
    public AudioSource PlayEarthquake() {
        return PlayNonSpatiallyAwareSound(earthquake, Camera.main.transform, 1f);
    }
    public void PlayC26Rumble() {
        PlayNonSpatiallyAwareSound(c26Rumble, Camera.main.transform, 1f);
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

    public void PlayForcePushJump(Transform spawnTransform) {
        PlayRandomSound(forcePushJump, spawnTransform, 1f);
    }
    public void PlayForcePushLand(Transform spawnTransform) {
        PlayRandomSound(forcePushLand, spawnTransform, 1f);
    }
    public void PlayForcePushExecute(Transform spawnTransform) {
        PlaySound(forcePushExecute, spawnTransform, 1f);
    }
    public AudioSource PlayForcePushStartCharging(Transform spawnTransform) {
        return PlaySound(forcePushStartCharging, spawnTransform, 1f);
    }
    public AudioSource PlayForcePushChargeLoop(Transform spawnTransform) {
        return PlayLoopedSound(forcePushChargeLoop, spawnTransform, 1f);
    }
    public void PlayPlayerGenericDeath(Transform spawnTransform) {
        if(CanPlaySound("playerGenericDeath", 0.5f))
            PlaySound(genericDeath, spawnTransform, 1f);
    }
    public void PlayPlayerShadowDeath(Transform spawnTransform) {
        if(CanPlaySound("playerShadowDeath", 0.5f))
            PlaySound(shadowDeath, spawnTransform, 1f);
    }
    public void PlayPlayerShadowSpawn(Transform spawnTransform) {
        if(CanPlaySound("playerShadowSpawn", 0.5f))
            PlaySound(shadowSpawn, spawnTransform, 1f);
    }
    public void PlayPlayerLongFall() {
        PlaySound(longFall, Camera.main.transform, 1f);
    }
    public void PlayPlayerLandHeavy() {
        PlaySound(landHeavy, Camera.main.transform, 1f);
    }
    public void PlayPlayerTeleportStart(Transform spawnTransform) {
        PlaySound(teleportStart, spawnTransform, 1f);
    }
    public void PlayPlayerTeleportEnd(Transform spawnTransform) {
        PlaySound(teleportEnd, spawnTransform, 1f);
    }
    public void PlayPlayerPickupCavePowerup(Transform spawnTransform) {
        PlayRandomSound(pickupCavePowerup, spawnTransform, 1f);
    }
    public void PlayPlayerShapeshiftToBlob(Transform spawnTransform) {
        PlayRandomSound(shapeshiftToBlob, spawnTransform, 1f);
    }
    public void PlayPlayerShapeshiftToHuman(Transform spawnTransform) {
        PlayRandomSound(shapeshiftToHuman, spawnTransform, 1f);
    }

    public AudioSource PlayBlockSliding(Transform spawnTransform, float soundDurationPercentage) {
        return PlaySound(blockSliding, spawnTransform, 1f, soundDurationPercentage);
    }
    public void PlayBlockLand(Transform spawnTransform) {
        if(CanPlaySound("blockLand", 0.5f))
            PlaySound(blockLand, spawnTransform, 1f);
    }
    public void PlayBlockSlideOffEdge(Transform spawnTransform) {
        PlayRandomSound(blockSlideOffEdge, spawnTransform, 1f);
    }
    public void PlayBlockWallImpact(Transform spawnTransform) {
        PlayRandomSound(blockWallImpact, spawnTransform, 1f);
    }

    public void PlayBreakableWallCrackling(Transform spawnTransform) {
        PlayRandomSound(breakableWallCrackling, spawnTransform, 1f);
    }

    public void PlayBreakableWallBreak(Transform spawnTransform) {
        PlaySound(breakableWallBreak, spawnTransform, 1f);
    }

    public void PlayBreakableWallHint(Transform spawnTransform) {
        PlaySound(breakableWallHint, spawnTransform, 1f);
    }

    public void PlayFallingSpikeCrackling(Transform spawnTransform) {
        PlayRandomSound(fallingSpikeCrackling, spawnTransform, 1f);
    }
    public void PlayFallingSpikeFall(Transform spawnTransform) {
        if (CanPlaySound("fallingSpikeFall", 0.5f)) {
            PlayRandomSound(fallingSpikeFall, spawnTransform, 1f);
        }
    }
    public void PlayFallingSpikeHit(Transform spawnTransform) {
        if (CanPlaySound("fallingSpikeHit", 0.5f)) {
            PlayRandomSound(fallingSpikeHit, spawnTransform, 1f);
        }
    }
    public void PlayFallingSpikeHitLiquid(Transform spawnTransform) {
        PlayRandomSound(fallingSpikeHitLiquid, spawnTransform, 1f);
    }
    public void PlayMushroomBigBounce(Transform spawnTransform) {
        PlayRandomSound(mushroomBigBounce, spawnTransform, 1f);
    }
    public void PlayMushroomSmallBounce(Transform spawnTransform) {
        PlayRandomSound(mushroomSmallBounce, spawnTransform, 1f);
    }
    public void PlayMushroomSmallRattle(Transform spawnTransform) {
        PlayRandomSound(mushroomSmallRattle, spawnTransform, 1f);
    }

    public void PlayBabyPrisonerCrawl(Transform spawnTransform) {
        PlayRandomSound(babyPrisonerCrawl, spawnTransform, 1f);
    }
    public void PlayBabyPrisonerIdle(Transform spawnTransform) {
        PlayRandomSound(babyPrisonerIdle, spawnTransform, 1f);
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
        if (CanPlaySound("prisonerSpawn", 0.5f)) {
            PlayRandomSound(prisonerSpawn, spawnTransform, 1f);
        }
    }
    public AudioSource PlayPrisonerHit(Transform spawnTransform) {
        return PlayLoopedSound(prisonerHit, spawnTransform, 1f);
    }
    public void PlayPrisonerSlide(Transform spawnTransform) {
        PlaySound(prisonerSlide, spawnTransform, 1f);
    }
    public void PlayPrisonerDeath(Transform spawnTransform) {
        PlayRandomSound(prisonerDeath, spawnTransform, 1f);
    }

    public void PlayMonsterDoorDestroy(Transform spawnTransform) {
        PlayRandomSound(monsterDoorDestroy, spawnTransform, 1f);
    }

    public void PlayPrisonerSoulAbsorb1(Transform spawnTransform) {
        PlaySound(prisonerSoulAbsorb1, spawnTransform, 1f);
    }    
    public void PlayPrisonerSoulAbsorb2(Transform spawnTransform) {
        PlaySound(prisonerSoulAbsorb2, spawnTransform, 1f);
    }    
    public void PlayPrisonerSoulAbsorb3(Transform spawnTransform) {
        PlaySound(prisonerSoulAbsorb3, spawnTransform, 1f);
    }

    public void PlayCapeIntroduction(Transform spawnTransform) {
        PlayNonSpatiallyAwareSound(capeIntroduction, spawnTransform, 1f);
    }
    public void PlayCapePickUp(Transform spawnTransform) {
        PlayNonSpatiallyAwareSound(capePickUp, spawnTransform, 1f);
    }

    public void PlayRevealSecret(Transform spawnTransform) {
        PlayNonSpatiallyAwareSound(revealSecret, spawnTransform, 1f);
    }

    public void PlayStartTransformingToBlobFirstTime(Transform spawnTransform) {
        PlayNonSpatiallyAwareSound(startTransformingToBlobFirstTime, spawnTransform, 1f);
    }

    public AudioSource PlaySound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(spatiallyAwareSoundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.parent = spawnTransform;
        audioSource.clip = clip;
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
        return audioSource;
    }

    public AudioSource PlayNonSpatiallyAwareSound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(nonSpatiallyAwareSoundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.parent = spawnTransform;
        audioSource.clip = clip;
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
        return audioSource;
    }

    public void PlayRandomNonSpatiallyAwareSound(AudioClip[] clips, Transform spawnTransform, float volume)
    {
        int random = GetRandomIndex(clips);
        PlayNonSpatiallyAwareSound(clips[random], spawnTransform, volume);
    }

    public AudioSource PlayLoopedSound(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(spatiallyAwareSoundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.transform.parent = spawnTransform;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
    }

    public void FadeOutAndStopSound(AudioSource audioSource, float duration) {
        StartCoroutine(FadeOutAndStopSoundCoroutine(audioSource, duration));
    }

    private IEnumerator FadeOutAndStopSoundCoroutine(AudioSource audioSource, float duration) {
        float currentTime = 0;
        float currentVol = audioSource.volume;
        float targetValue = 0.0001f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            if(audioSource == null)
                yield break;
            audioSource.volume = newVol;
            yield return null;
        }

        if(audioSource != null) {
            audioSource.Stop();
            Destroy(audioSource.gameObject);
        }

        yield break;
    }

    public AudioSource PlaySound(AudioClip clip, Transform spawnTransform, float volume, float clipLengthPercentage)
    {
        AudioSource audioSource = Instantiate(spatiallyAwareSoundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        float clipLength = audioSource.clip.length * clipLengthPercentage;
        PlaySound(audioSource, clipLength);
        return audioSource;
    }

    public void PlayRandomSound(AudioClip[] clips, Transform spawnTransform, float volume)
    {
        int random = GetRandomIndex(clips);

        AudioSource audioSource = Instantiate(spatiallyAwareSoundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clips[random];
        audioSource.volume = volume;
        PlaySound(audioSource, audioSource.clip.length);
    }

    //Used to make sure that we don't play the same sound clip two times in a row
    private int GetRandomIndex(AudioClip[] clips) {
        if (!lastPickedIndices.ContainsKey(clips))
        {
            lastPickedIndices[clips] = -1;
        }

        int lastPickedIndex = lastPickedIndices[clips];
        int newIndex;

        do
        {
            newIndex = Random.Range(0, clips.Length);
        }
        while (newIndex == lastPickedIndex && clips.Length > 1);

        lastPickedIndices[clips] = newIndex;
        return newIndex;
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
