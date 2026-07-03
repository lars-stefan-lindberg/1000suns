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
    private Transform _mainCameraTransform;
    private PixelPerfectCamera _pixelPerfectCamera;
    private Transform _transform;
    
    private int _cachedAssetsPixelsPerUnit;
    private float _cachedInversePixelsPerUnit;
    private float _rootSmoothTimeInverse;
    private float _layerSmoothTimeInverse;
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
    [SerializeField] private bool _disableYParallax = false;
    [SerializeField] private bool _lockBackgroundYPosition = false;

    private Vector3 _cameraStartPosition;
    private Vector3 _backStartLocalPosition;
    private Vector3 _frontStartLocalPosition;
    private float _backOriginalLocalY;
    private float _frontOriginalLocalY;
    private float _managerOriginalLocalY;
    private float _managerOriginalWorldY;

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

    private void CachePixelPerfectValues()
    {
        _cachedAssetsPixelsPerUnit = GetAssetsPixelsPerUnit();
        _cachedInversePixelsPerUnit = 1f / _cachedAssetsPixelsPerUnit;
    }

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _mainCameraTransform = _mainCamera.transform;
            _pixelPerfectCamera = _mainCamera.GetComponent<PixelPerfectCamera>();
        }
        _transform = transform;
        
        CachePixelPerfectValues();
        _rootSmoothTimeInverse = 1f / _rootSmoothTime;
        _layerSmoothTimeInverse = 1f / _layerSmoothTime;
        _startPosition = transform.position;
        
        _managerOriginalLocalY = _transform.localPosition.y;
        _managerOriginalWorldY = _transform.position.y;
        
        if (_backgroundBack != null)
        {
            _backOriginalLocalY = _backgroundBack.localPosition.y;
        }
        
        if (_backgroundFront != null)
        {
            _frontOriginalLocalY = _backgroundFront.localPosition.y;
        }

        if (!_isChildOfMainCamera && _mainCamera != null)
        {
            _isChildOfMainCamera = _transform.parent == _mainCameraTransform;
        }

        if (_mainCamera != null && !_sharedParallaxOriginInitialized)
        {
            Vector3 camPos = _mainCameraTransform.position;
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
                _mainCameraTransform = _mainCamera.transform;
                _pixelPerfectCamera = _mainCamera.GetComponent<PixelPerfectCamera>();
                CachePixelPerfectValues();
            }
        }

        if (_transform == null)
        {
            _transform = transform;
        }

        if (!_isChildOfMainCamera && _mainCamera != null)
        {
            _isChildOfMainCamera = _transform.parent == _mainCameraTransform;
        }

        if (_mainCamera != null)
        {
            Vector3 camPos = _mainCameraTransform.position;
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
        
        if (_managerOriginalLocalY == 0f && _managerOriginalWorldY == 0f)
        {
            _managerOriginalLocalY = _transform.localPosition.y;
            _managerOriginalWorldY = _transform.position.y;
        }

        if (_backgroundBack != null)
        {
            _backStartLocalPosition = _backgroundBack.localPosition;
            if (_backOriginalLocalY == 0f)
            {
                _backOriginalLocalY = _backgroundBack.localPosition.y;
            }
            _backSmoothedLocalPosition = _backgroundBack.localPosition;
            _backLastSnappedPixels = WorldToPixel(_backSmoothedLocalPosition);
        }

        if (_backgroundFront != null)
        {
            _frontStartLocalPosition = _backgroundFront.localPosition;
            if (_frontOriginalLocalY == 0f)
            {
                _frontOriginalLocalY = _backgroundFront.localPosition.y;
            }
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
            if (_lockBackgroundYPosition)
            {
                rootLocal.y = _managerOriginalWorldY - _mainCameraTransform.position.y;
            }
            else
            {
                rootLocal.y = 0f;
            }
            _transform.localPosition = rootLocal;

            _rootSmoothedPosition = _transform.position;
            _rootLastSnappedPixels = WorldToPixel(_rootSmoothedPosition);
        }

        Vector3 cameraPosition = _mainCameraTransform.position;
        if (!_cameraPositionAlreadySnapped)
        {
            cameraPosition = SnapToPixelGrid(cameraPosition);
        }

        Vector3 cameraDelta = cameraPosition - _cameraStartPosition;

        if (!_isChildOfMainCamera)
        {
            float targetY = _lockBackgroundYPosition ? _managerOriginalWorldY : cameraPosition.y;
            Vector3 rootTarget = new Vector3(cameraPosition.x, targetY, _transform.position.z);
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
            if(!_disableYParallax && !_lockBackgroundYPosition)
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
            if(!_disableYParallax && !_lockBackgroundYPosition)
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
        return new Vector2Int(
            Mathf.RoundToInt(position.x * _cachedAssetsPixelsPerUnit),
            Mathf.RoundToInt(position.y * _cachedAssetsPixelsPerUnit)
        );
    }

    private Vector3 PixelToWorld(Vector2Int pixels, float z)
    {
        return new Vector3(pixels.x * _cachedInversePixelsPerUnit, pixels.y * _cachedInversePixelsPerUnit, z);
    }

    private Vector3 SnapToPixelGridHysteresis(Vector3 position, ref Vector2Int lastSnappedPixels)
    {
        float xPixels = position.x * _cachedAssetsPixelsPerUnit;
        float yPixels = position.y * _cachedAssetsPixelsPerUnit;

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
        int x = Mathf.RoundToInt(position.x * _cachedAssetsPixelsPerUnit);
        int y = Mathf.RoundToInt(position.y * _cachedAssetsPixelsPerUnit);

        position.x = x * _cachedInversePixelsPerUnit;
        position.y = y * _cachedInversePixelsPerUnit;
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

        Vector3 cameraPosition = _mainCameraTransform.position;
        if (!_cameraPositionAlreadySnapped)
        {
            cameraPosition = SnapToPixelGrid(cameraPosition);
        }

        Vector3 cameraDelta = cameraPosition - _cameraStartPosition;

        if (_isChildOfMainCamera && _lockBackgroundYPosition)
        {
            Vector3 rootLocal = _transform.localPosition;
            rootLocal.y = _managerOriginalWorldY - _mainCameraTransform.position.y;
            _transform.localPosition = rootLocal;
        }

        if (!_isChildOfMainCamera)
        {
            float targetY = _lockBackgroundYPosition ? _managerOriginalWorldY : cameraPosition.y;
            Vector3 rootTarget = new Vector3(cameraPosition.x, targetY, _transform.position.z);
            if (_smoothRootFollow)
            {
                _rootSmoothedPosition = Vector3.SmoothDamp(
                    _rootSmoothedPosition,
                    rootTarget,
                    ref _rootVelocity,
                    _rootSmoothTimeInverse
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
            if(!_disableYParallax && !_lockBackgroundYPosition)
                targetLocal.y += -cameraDelta.y * _parallaxBackY;

            _backSmoothedLocalPosition = Vector3.SmoothDamp(
                _backSmoothedLocalPosition,
                targetLocal,
                ref _backVelocity,
                _layerSmoothTimeInverse
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
            if(!_disableYParallax && !_lockBackgroundYPosition)
                targetLocal.y += -cameraDelta.y * _parallaxFrontY;

            _frontSmoothedLocalPosition = Vector3.SmoothDamp(
                _frontSmoothedLocalPosition,
                targetLocal,
                ref _frontVelocity,
                _layerSmoothTimeInverse
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
