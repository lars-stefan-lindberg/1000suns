using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 5f;

    private bool _isTransitioning = false;
    private Transform _lastActiveTransform = null;

    void Update()
    {
        Transform activeTransform = PlayerManager.obj.GetPlayerTransform();
        if (activeTransform == null) return;

        // Detect transformation event
        if (_lastActiveTransform != null && activeTransform != _lastActiveTransform)
        {
            _isTransitioning = true;
        }

        if (_isTransitioning)
        {
            Vector3 targetPos = activeTransform.position;
            _light.transform.position = Vector3.Lerp(_light.transform.position, targetPos, Time.deltaTime * _transitionSpeed);
            // End transition if close enough to the current position
            if (Vector3.Distance(_light.transform.position, targetPos) < 0.05f)
            {
                _light.transform.position = targetPos;
                _isTransitioning = false;
            }
        }
        else
        {
            _light.transform.position = activeTransform.position;
        }

        _lastActiveTransform = activeTransform;
    }
}
