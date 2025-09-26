using System.Collections;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager obj;

    public enum AmbienceId {
        None,
        Cave,
        CapeRoom
    }

    private AmbienceId _currentAmbienceId = AmbienceId.None;

    [SerializeField] private AudioSource ambienceObject;

    public AudioClip caveAmbience;
    public AudioClip capeRoomAmbience1;
    public AudioClip capeRoomAmbience2;

    private AudioSource _ambienceSourceLayer1;
    private AudioSource _ambienceSourceLayer2;
    private AudioSource _ambienceSourceLayer3;

    void Start() {
        obj = this;
    }

    [ContextMenu("Play cave ambience")]
    public void PlayCaveAmbience() {
        _ambienceSourceLayer1 = Instantiate(ambienceObject, Camera.main.transform.position, Quaternion.identity);
        _ambienceSourceLayer1.transform.parent = transform;
        _ambienceSourceLayer1.clip = caveAmbience;
        _ambienceSourceLayer1.volume = 0f;
        _ambienceSourceLayer1.loop = true;
        _ambienceSourceLayer1.Play();
        _currentAmbienceId = AmbienceId.Cave;
    }

    [ContextMenu("Play cape room ambience")]
    public void PlayCapeRoomAmbience() {
        _ambienceSourceLayer2 = Instantiate(ambienceObject, Camera.main.transform.position, Quaternion.identity);
        _ambienceSourceLayer2.transform.parent = transform;
        _ambienceSourceLayer3 = Instantiate(ambienceObject, Camera.main.transform.position, Quaternion.identity);
        _ambienceSourceLayer3.transform.parent = transform;

        _ambienceSourceLayer2.clip = capeRoomAmbience1;
        _ambienceSourceLayer2.playOnAwake = false;
        _ambienceSourceLayer2.loop = true;
        _ambienceSourceLayer2.volume = 0f;

        _ambienceSourceLayer3.clip = capeRoomAmbience2;
        _ambienceSourceLayer3.playOnAwake = false;
        _ambienceSourceLayer3.loop = true;
        _ambienceSourceLayer3.volume = 0f;

        double startTime = AudioSettings.dspTime + 1;
        _ambienceSourceLayer2.PlayScheduled(startTime);
        _ambienceSourceLayer3.PlayScheduled(startTime);
        _currentAmbienceId = AmbienceId.CapeRoom;
    }

    public void FadeOutAmbienceSource1(float duration) {
        StartCoroutine(FadeOutAmbienceSource(_ambienceSourceLayer1, duration));
    }
    public void FadeInAmbienceSource1(float duration) {
        StartCoroutine(FadeInAmbienceSource(_ambienceSourceLayer1, duration));
    }
    public void FadeInAmbienceSource2And3(float duration) {
        StartCoroutine(FadeInAmbienceSource(_ambienceSourceLayer2, duration));
        StartCoroutine(FadeInAmbienceSource(_ambienceSourceLayer3, duration));
    }
    public void FadeOutAmbienceSource2And3(float duration) {
        StartCoroutine(FadeOutAmbienceSource(_ambienceSourceLayer2, duration));
        StartCoroutine(FadeOutAmbienceSource(_ambienceSourceLayer3, duration));
    }
    public bool IsAmbienceSource1Playing() {
        if(_ambienceSourceLayer1 == null)
            return false;
        return _ambienceSourceLayer1.isPlaying;
    }
    public bool IsAmbienceSource2Playing() {
        if(_ambienceSourceLayer2 == null)
            return false;
        return _ambienceSourceLayer2.isPlaying;
    }
    public float GetAmbienceSource1Volume() {
        return _ambienceSourceLayer1.volume;
    }
    public float GetAmbienceSource2Volume() {
        return _ambienceSourceLayer2.volume;
    }
    public float GetAmbienceSource3Volume() {
        return _ambienceSourceLayer3.volume;
    }

    private IEnumerator FadeOutAmbienceSource(AudioSource audioSource, float duration) {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(1, 0, currentTime / duration);
            audioSource.volume = newVol;
            yield return null;
        }
        audioSource.Stop();
        Destroy(audioSource.gameObject);
        yield break;
    }
    private IEnumerator FadeInAmbienceSource(AudioSource audioSource, float duration) {
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(0, 1, currentTime / duration);
            audioSource.volume = newVol;
            yield return null;
        }
        yield break;
    }

    public void StopAmbience() {
        if(_ambienceSourceLayer1 != null) {
            _ambienceSourceLayer1.Stop();
            Destroy(_ambienceSourceLayer1.gameObject);
        }
        if(_ambienceSourceLayer2 != null) {
            _ambienceSourceLayer2.Stop();
            Destroy(_ambienceSourceLayer2.gameObject);
        }
        if(_ambienceSourceLayer3 != null) {
            _ambienceSourceLayer3.Stop();
            Destroy(_ambienceSourceLayer3.gameObject);
        }
        _currentAmbienceId = AmbienceId.None;
    }

    public string GetCurrentAmbienceId() {
        return _currentAmbienceId == AmbienceId.None ? null : _currentAmbienceId.ToString();
    }

    public void SetCurrentAmbienceId(AmbienceId ambienceId) {
        _currentAmbienceId = ambienceId;
    }

    public void PlayById(string id) {
        if (string.IsNullOrEmpty(id)) return;
        if (!System.Enum.TryParse<AmbienceId>(id, out var ambienceId)) return;
        switch (ambienceId) {
            case AmbienceId.Cave: 
                PlayCaveAmbience(); 
                FadeInAmbienceSource1(2f);
            break;
            case AmbienceId.CapeRoom: 
                PlayCapeRoomAmbience(); 
                FadeInAmbienceSource2And3(2f);
            break;
            default: break;
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
