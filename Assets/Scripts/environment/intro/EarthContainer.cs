using UnityEngine;

public class EarthContainer : MonoBehaviour
{
    [SerializeField] private ParticleSystem _windParticles;
    [SerializeField] private GameObject _windParticleCollider;
    [SerializeField] private ParticleSystem _oceanParticles;
    [SerializeField] private ParticleSystem _starParticles;
    [SerializeField] private GameObject _sunLight;
    [SerializeField] private GameObject _earthLight;
    [SerializeField] private GameObject _earthBodyLight;
    [SerializeField] private GameObject[] _clouds;

    public void OnZoomOutStart() {
        _starParticles.gameObject.SetActive(true);
        _sunLight.SetActive(true);
        _earthLight.SetActive(true);
        foreach(GameObject cloud in _clouds) 
            cloud.SetActive(true);
        _windParticleCollider.SetActive(true);
        _earthBodyLight.SetActive(true);
        _windParticles.gameObject.SetActive(true);
        _oceanParticles.gameObject.SetActive(true);
    }
    
    public void OnZoomedOutCompleted() {
        
    }

    public void PrepZoomIn() {
        
    }

    public void OnZoomInInProgress() {
        _starParticles.Stop();
        _windParticleCollider.SetActive(false);
        _sunLight.SetActive(false);
        _earthLight.SetActive(false);
        _earthBodyLight.SetActive(false);
        _windParticles.Stop();
        _oceanParticles.Stop();
    }

    public void OnZoomInCompleted() {
        gameObject.SetActive(false);
    }
}
