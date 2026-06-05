using UnityEngine;

public class DialogueAudioAnchor : MonoBehaviour
{
    [SerializeField] private float _leftOffsetX = -3f;
    [SerializeField] private float _rightOffsetX = 3f;

    private GameObject _leftAnchor;
    private GameObject _rightAnchor;
    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = Camera.main;

        _leftAnchor = new GameObject("DialogueAudioAnchor_Left");
        _leftAnchor.transform.SetParent(transform);

        _rightAnchor = new GameObject("DialogueAudioAnchor_Right");
        _rightAnchor.transform.SetParent(transform);

        UpdateAnchorPositions();
    }

    void LateUpdate()
    {
        UpdateAnchorPositions();
    }

    private void UpdateAnchorPositions()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
        }

        Vector3 cameraPos = _mainCamera.transform.position;

        _leftAnchor.transform.position = new Vector3(cameraPos.x + _leftOffsetX, cameraPos.y, cameraPos.z);
        _rightAnchor.transform.position = new Vector3(cameraPos.x + _rightOffsetX, cameraPos.y, cameraPos.z);
    }

    public GameObject GetLeftAnchor()
    {
        return _leftAnchor;
    }

    public GameObject GetRightAnchor()
    {
        return _rightAnchor;
    }

    void OnDestroy()
    {
        if (_leftAnchor != null)
            Destroy(_leftAnchor);
        if (_rightAnchor != null)
            Destroy(_rightAnchor);
    }
}
