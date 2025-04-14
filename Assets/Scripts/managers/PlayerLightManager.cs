using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    [SerializeField] private GameObject _light;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _blobTransform;
    [SerializeField] private float _transitionSpeed = 5f;
    
    private bool _isTransitioning = false;
    private Vector3 _targetPosition;

    void Update()
    {
        //TODO make this work
        // Transform activeTransform = PlayerManager.obj.GetPlayerTransform();
        
        // // If we just switched characters, start a transition
        // if (activeTransform != null && (_playerTransform != null && activeTransform == _playerTransform && _isTransitioning == false && _light.transform.position != _playerTransform.position) ||
        //     (_blobTransform != null && activeTransform == _blobTransform && _isTransitioning == false && _light.transform.position != _blobTransform.position))
        // {
        //     _isTransitioning = true;
        //     _targetPosition = activeTransform.position;
        // }
        
        // // Handle the transition
        // if (_isTransitioning)
        // {
        //     _light.transform.position = Vector3.Lerp(_light.transform.position, _targetPosition, Time.deltaTime * _transitionSpeed);
            
        //     // Check if we're close enough to end the transition
        //     if (Vector3.Distance(_light.transform.position, _targetPosition) < 0.05f)
        //     {
        //         _isTransitioning = false;
        //     }
        // }
        // else if (activeTransform != null)
        // {
        //     // When not transitioning, just update the target position to follow the active player
        //     _targetPosition = activeTransform.position;
        //     _light.transform.position = _targetPosition;
        // }
        _light.transform.position = PlayerManager.obj.GetPlayerTransform().position;
    }
}
