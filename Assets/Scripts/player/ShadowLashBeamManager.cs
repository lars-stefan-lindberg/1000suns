using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowLashBeamManager : MonoBehaviour
{
    public static ShadowLashBeamManager obj;
    
    [SerializeField] private GameObject _shadowLashBeamPrefabRight;
    [SerializeField] private GameObject _shadowLashBeamPrefabLeft;
    [SerializeField] private GameObject _shadowLashBeamPrefabUp;

    private ShadowLashBeam _activeBeam;
    private GameObject _activeBeamObject;
    private bool _isTrackingBeam;

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }

    void Update() {
        if (_isTrackingBeam && _activeBeam != null && ShadowTwinMovement.obj != null)
        {
            // Update tail mask to follow player's horizontal position
            _activeBeam.UpdateTailPosition(ShadowTwinMovement.obj.transform.position);

            // Check if player has latched to surface
            if (ShadowTwinMovement.obj.IsLatchedToSurface())
            {
                _isTrackingBeam = false;
                StartCoroutine(DestroyBeamAfterParticles(_activeBeam, _activeBeamObject));
                _activeBeam = null;
                _activeBeamObject = null;
            }
        }
    }

    public void ForceStopBeam() {
        if (_activeBeam != null) {
            _isTrackingBeam = false;
            StartCoroutine(DestroyBeamAfterParticles(_activeBeam, _activeBeamObject));
            _activeBeam = null;
            _activeBeamObject = null;
        }
    }

    private IEnumerator DestroyBeamAfterParticles(ShadowLashBeam beam, GameObject beamObject)
    {
        yield return beam.DisableBeamAndWaitForParticles();
        Destroy(beamObject);
    }

    public void ShootBeam(Vector3 spawnLocation, Vector2 lashDirection) {
        GameObject prefabToUse;
        
        // Determine which prefab to use based on lash direction
        if (Mathf.Abs(lashDirection.y) > 0)
        {
            // Vertical lash (up or down)
            prefabToUse = _shadowLashBeamPrefabUp;
        }
        else
        {
            // Horizontal lash (left or right)
            prefabToUse = lashDirection.x > 0 ? _shadowLashBeamPrefabRight : _shadowLashBeamPrefabLeft;
        }
        
        GameObject shadowLashBeamPrefab = Instantiate(prefabToUse, spawnLocation, transform.rotation);
        ShadowLashBeam shadowLashBeamComponent = shadowLashBeamPrefab.GetComponent<ShadowLashBeam>();
        shadowLashBeamComponent.Lash(lashDirection);
        
        _activeBeam = shadowLashBeamComponent;
        _activeBeamObject = shadowLashBeamPrefab;
        _isTrackingBeam = true;
    }
    
    public void StopBeam() {
        _isTrackingBeam = false;
        if (_activeBeam != null) {
            StartCoroutine(StopBeamCoroutine(_activeBeam, _activeBeamObject));
        }
    }

    private IEnumerator StopBeamCoroutine(ShadowLashBeam beam, GameObject beamObject) {
        yield return beam.FadeOut();
        Destroy(beamObject, 1f); //Allow time for eventual particles to fade out
    }

    public void TriggerHitSurfaceAnimation() {
        _activeBeam?.TriggerHitSurfaceAnimation();
    }
}
