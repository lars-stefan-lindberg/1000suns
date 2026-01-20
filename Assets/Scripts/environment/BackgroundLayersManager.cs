using UnityEngine;
using UnityEngine.U2D;

/*
This manager lives in it's own scene. Each room has a scene reference for the background layers that should be loaded.
When the layers are loaded, they are added as a child to the main camera to enable smooth parallax effect.
Depending on the room, the background layers will be moved into "parallaxed" position relative to the starting position of the layers,
ensuring that the same background will always be shown in each specific room.
*/
public class BackgroundLayersManager : MonoBehaviour
{
    private static bool _sharedParallaxOriginInitialized = false;
    private static Vector3 _sharedParallaxOriginCameraPosition;

    private Camera _mainCamera;
    private PixelPerfectCamera _pixelPerfectCamera;
    private Transform _transform;
    private Vector3 _startPosition;
    public bool isActive = false;
    public SceneField backgroundScene;
    [SerializeField] private Transform _backgroundBack;
    [SerializeField] private Transform _backgroundFront;

    [SerializeField] private float _rootSmoothTime = 12f;
    [SerializeField] private float _layerSmoothTime = 8f;

    [SerializeField] private float _snapHysteresisPixels = 0.1f;

    [SerializeField] private bool _cameraPositionAlreadySnapped = true;
    [SerializeField] private bool _smoothRootFollow = false;
    [SerializeField] private bool _useHysteresisSnapping = false;

    [SerializeField] private bool _isChildOfMainCamera = false;

    [SerializeField] private bool _snapCameraTransform = false;
    [SerializeField] private bool _overrideAssetsPixelsPerUnit = false;
    [SerializeField] private int _assetsPixelsPerUnitOverride = 16;

    [SerializeField] private float _parallaxBackX = 0.05f;
    [SerializeField] private float _parallaxBackY = 0.02f;
    [SerializeField] private float _parallaxFrontX = 0.1f;
    [SerializeField] private float _parallaxFrontY = 0.04f;

    private Vector3 _cameraStartPosition;
    private Vector3 _backStartLocalPosition;
    private Vector3 _frontStartLocalPosition;

    private Vector3 _rootVelocity;
    private Vector3 _backVelocity;
    private Vector3 _frontVelocity;

    private Vector3 _rootSmoothedPosition;
    private Vector3 _backSmoothedLocalPosition;
    private Vector3 _frontSmoothedLocalPosition;

    private Vector2Int _rootLastSnappedPixels;
    private Vector2Int _backLastSnappedPixels;
    private Vector2Int _frontLastSnappedPixels;

    private bool _activationInitialized = false;
    private bool _wasActiveLastFrame = false;

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _pixelPerfectCamera = _mainCamera.GetComponent<PixelPerfectCamera>();
        }
        _transform = transform;
        _startPosition = transform.position;

        if (!_isChildOfMainCamera && _mainCamera != null)
        {
            _isChildOfMainCamera = _transform.parent == _mainCamera.transform;
        }

        if (_mainCamera != null && !_sharedParallaxOriginInitialized)
        {
            Vector3 camPos = _mainCamera.transform.position;
            Vector3 snappedCamPos = _cameraPositionAlreadySnapped ? camPos : SnapToPixelGrid(camPos);
            _sharedParallaxOriginInitialized = true;
            _sharedParallaxOriginCameraPosition = snappedCamPos;
        }

        _wasActiveLastFrame = isActive;
    }

    private void EnsureInitializedForActivation()
    {
        if (_activationInitialized)
        {
            return;
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                _pixelPerfectCamera = _mainCamera.GetComponent<PixelPerfectCamera>();
            }
        }

        if (_transform == null)
        {
            _transform = transform;
        }

        if (!_isChildOfMainCamera && _mainCamera != null)
        {
            _isChildOfMainCamera = _transform.parent == _mainCamera.transform;
        }

        if (_mainCamera != null)
        {
            Vector3 camPos = _mainCamera.transform.position;
            Vector3 snappedCamPos = _cameraPositionAlreadySnapped ? camPos : SnapToPixelGrid(camPos);
            if (!_sharedParallaxOriginInitialized)
            {
                _sharedParallaxOriginInitialized = true;
                _sharedParallaxOriginCameraPosition = snappedCamPos;
            }

            if (_isChildOfMainCamera)
            {
                Vector3 rootPos = _transform.position;
                _cameraStartPosition = _cameraPositionAlreadySnapped ? rootPos : SnapToPixelGrid(rootPos);
            }
            else
            {
                _cameraStartPosition = _sharedParallaxOriginCameraPosition;
            }
        }

        _startPosition = _transform.position;
        _rootSmoothedPosition = _transform.position;
        _rootLastSnappedPixels = WorldToPixel(_rootSmoothedPosition);

        if (_backgroundBack != null)
        {
            _backStartLocalPosition = _backgroundBack.localPosition;
            _backSmoothedLocalPosition = _backgroundBack.localPosition;
            _backLastSnappedPixels = WorldToPixel(_backSmoothedLocalPosition);
        }

        if (_backgroundFront != null)
        {
            _frontStartLocalPosition = _backgroundFront.localPosition;
            _frontSmoothedLocalPosition = _backgroundFront.localPosition;
            _frontLastSnappedPixels = WorldToPixel(_frontSmoothedLocalPosition);
        }

        _activationInitialized = true;
    }

    private void ApplyImmediateParallaxToCurrentCameraPosition()
    {
        if (_mainCamera == null)
        {
            return;
        }

        if (_isChildOfMainCamera)
        {
            Vector3 rootLocal = _transform.localPosition;
            rootLocal.x = 0f;
            rootLocal.y = 0f;
            _transform.localPosition = rootLocal;

            _rootSmoothedPosition = _transform.position;
            _rootLastSnappedPixels = WorldToPixel(_rootSmoothedPosition);
        }

        Vector3 cameraPosition = _mainCamera.transform.position;
        if (!_cameraPositionAlreadySnapped)
        {
            cameraPosition = SnapToPixelGrid(cameraPosition);
        }

        Vector3 cameraDelta = cameraPosition - _cameraStartPosition;

        if (!_isChildOfMainCamera)
        {
            Vector3 rootTarget = new Vector3(cameraPosition.x, cameraPosition.y, _transform.position.z);
            _rootSmoothedPosition = rootTarget;
            if (_useHysteresisSnapping)
            {
                _transform.position = SnapToPixelGridHysteresis(_rootSmoothedPosition, ref _rootLastSnappedPixels);
            }
            else
            {
                _transform.position = SnapToPixelGrid(_rootSmoothedPosition);
            }
        }

        if (_backgroundBack != null)
        {
            Vector3 targetLocal = _backStartLocalPosition;
            targetLocal.x += -cameraDelta.x * _parallaxBackX;
            targetLocal.y += -cameraDelta.y * _parallaxBackY;

            _backSmoothedLocalPosition = targetLocal;
            if (_useHysteresisSnapping)
            {
                _backgroundBack.localPosition = SnapToPixelGridHysteresis(_backSmoothedLocalPosition, ref _backLastSnappedPixels);
            }
            else
            {
                _backgroundBack.localPosition = SnapToPixelGrid(_backSmoothedLocalPosition);
            }
        }

        if (_backgroundFront != null)
        {
            Vector3 targetLocal = _frontStartLocalPosition;
            targetLocal.x += -cameraDelta.x * _parallaxFrontX;
            targetLocal.y += -cameraDelta.y * _parallaxFrontY;

            _frontSmoothedLocalPosition = targetLocal;
            if (_useHysteresisSnapping)
            {
                _backgroundFront.localPosition = SnapToPixelGridHysteresis(_frontSmoothedLocalPosition, ref _frontLastSnappedPixels);
            }
            else
            {
                _backgroundFront.localPosition = SnapToPixelGrid(_frontSmoothedLocalPosition);
            }
        }
    }

    private float GetPixelSizeWorld()
    {
        if (_pixelPerfectCamera != null && _pixelPerfectCamera.assetsPPU > 0)
        {
            return 1f / _pixelPerfectCamera.assetsPPU;
        }

        return 1f / 16f;
    }

    private int GetAssetsPixelsPerUnit()
    {
        if (_overrideAssetsPixelsPerUnit && _assetsPixelsPerUnitOverride > 0)
        {
            return _assetsPixelsPerUnitOverride;
        }

        if (_pixelPerfectCamera != null && _pixelPerfectCamera.assetsPPU > 0)
        {
            return _pixelPerfectCamera.assetsPPU;
        }

        return 16;
    }

    private Vector2Int WorldToPixel(Vector3 position)
    {
        int ppu = GetAssetsPixelsPerUnit();
        return new Vector2Int(
            Mathf.RoundToInt(position.x * ppu),
            Mathf.RoundToInt(position.y * ppu)
        );
    }

    private Vector3 PixelToWorld(Vector2Int pixels, float z)
    {
        int ppu = GetAssetsPixelsPerUnit();
        return new Vector3((float)pixels.x / ppu, (float)pixels.y / ppu, z);
    }

    private Vector3 SnapToPixelGridHysteresis(Vector3 position, ref Vector2Int lastSnappedPixels)
    {
        int ppu = GetAssetsPixelsPerUnit();
        float xPixels = position.x * ppu;
        float yPixels = position.y * ppu;

        float margin = Mathf.Max(0f, _snapHysteresisPixels);

        if (xPixels > lastSnappedPixels.x + 0.5f + margin)
        {
            lastSnappedPixels.x = Mathf.FloorToInt(xPixels + 0.5f);
        }
        else if (xPixels < lastSnappedPixels.x - 0.5f - margin)
        {
            lastSnappedPixels.x = Mathf.CeilToInt(xPixels - 0.5f);
        }

        if (yPixels > lastSnappedPixels.y + 0.5f + margin)
        {
            lastSnappedPixels.y = Mathf.FloorToInt(yPixels + 0.5f);
        }
        else if (yPixels < lastSnappedPixels.y - 0.5f - margin)
        {
            lastSnappedPixels.y = Mathf.CeilToInt(yPixels - 0.5f);
        }

        return PixelToWorld(lastSnappedPixels, position.z);
    }

    private Vector3 SnapToPixelGrid(Vector3 position)
    {
        int ppu = GetAssetsPixelsPerUnit();

        int x = Mathf.RoundToInt(position.x * ppu);
        int y = Mathf.RoundToInt(position.y * ppu);

        position.x = (float)x / ppu;
        position.y = (float)y / ppu;
        return position;
    }

    void LateUpdate()
    {
        if (_mainCamera == null)
        {
            return;
        }

        if(!isActive) {
            _wasActiveLastFrame = false;
            return;
        }

        if (!_wasActiveLastFrame)
        {
            EnsureInitializedForActivation();
            ApplyImmediateParallaxToCurrentCameraPosition();
        }

        _wasActiveLastFrame = true;

        Vector3 cameraPosition = _mainCamera.transform.position;
        if (!_cameraPositionAlreadySnapped)
        {
            cameraPosition = SnapToPixelGrid(cameraPosition);
        }

        Vector3 cameraDelta = cameraPosition - _cameraStartPosition;

        if (!_isChildOfMainCamera)
        {
            Vector3 rootTarget = new Vector3(cameraPosition.x, cameraPosition.y, _transform.position.z);
            if (_smoothRootFollow)
            {
                _rootSmoothedPosition = Vector3.SmoothDamp(
                    _rootSmoothedPosition,
                    rootTarget,
                    ref _rootVelocity,
                    1f / _rootSmoothTime
                );
            }
            else
            {
                _rootSmoothedPosition = rootTarget;
            }

            if (_useHysteresisSnapping)
            {
                _transform.position = SnapToPixelGridHysteresis(_rootSmoothedPosition, ref _rootLastSnappedPixels);
            }
            else
            {
                _transform.position = SnapToPixelGrid(_rootSmoothedPosition);
            }
        }

        if (_backgroundBack != null)
        {
            Vector3 targetLocal = _backStartLocalPosition;
            targetLocal.x += -cameraDelta.x * _parallaxBackX;
            targetLocal.y += -cameraDelta.y * _parallaxBackY;

            _backSmoothedLocalPosition = Vector3.SmoothDamp(
                _backSmoothedLocalPosition,
                targetLocal,
                ref _backVelocity,
                1f / _layerSmoothTime
            );

            if (_useHysteresisSnapping)
            {
                _backgroundBack.localPosition = SnapToPixelGridHysteresis(_backSmoothedLocalPosition, ref _backLastSnappedPixels);
            }
            else
            {
                _backgroundBack.localPosition = SnapToPixelGrid(_backSmoothedLocalPosition);
            }
        }

        if (_backgroundFront != null)
        {
            Vector3 targetLocal = _frontStartLocalPosition;
            targetLocal.x += -cameraDelta.x * _parallaxFrontX;
            targetLocal.y += -cameraDelta.y * _parallaxFrontY;

            _frontSmoothedLocalPosition = Vector3.SmoothDamp(
                _frontSmoothedLocalPosition,
                targetLocal,
                ref _frontVelocity,
                1f / _layerSmoothTime
            );

            if (_useHysteresisSnapping)
            {
                _backgroundFront.localPosition = SnapToPixelGridHysteresis(_frontSmoothedLocalPosition, ref _frontLastSnappedPixels);
            }
            else
            {
                _backgroundFront.localPosition = SnapToPixelGrid(_frontSmoothedLocalPosition);
            }
        }
    }
}
