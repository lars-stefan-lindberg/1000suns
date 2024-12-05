using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager obj;

    [SerializeField] private AudioSource ambienceObject;

    public AudioClip caveAmbience;

    private AudioSource _ambienceSource;

    void Start() {
        obj = this;
    }

    [ContextMenu("Play ambience")]
    public void PlayAmbience() {
        _ambienceSource = Instantiate(ambienceObject, Camera.main.transform.position, Quaternion.identity);
        _ambienceSource.transform.parent = transform;
        _ambienceSource.clip = caveAmbience;
        _ambienceSource.volume = 1f;
        _ambienceSource.loop = true;
        _ambienceSource.Play();
    }
    
    public void StopAmbience() {
        if(_ambienceSource != null) {
            _ambienceSource.Stop();
            Destroy(_ambienceSource.gameObject);
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
