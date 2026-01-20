using Cinemachine;
using UnityEngine;
using UnityEngine.U2D;

public class CinemachinePixelSnapExtension : CinemachineExtension
{
    private Camera _mainCamera;
    private PixelPerfectCamera _pixelPerfectCamera;

    [SerializeField] private bool _overrideAssetsPixelsPerUnit = false;
    [SerializeField] private int _assetsPixelsPerUnitOverride = 16;

    protected override void Awake()
    {
        base.Awake();

        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _pixelPerfectCamera = _mainCamera.GetComponent<PixelPerfectCamera>();
        }
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

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
        {
            return;
        }

        int ppu = GetAssetsPixelsPerUnit();
        if (ppu <= 0)
        {
            return;
        }

        Vector3 pos = state.FinalPosition;

        int x = Mathf.RoundToInt(pos.x * ppu);
        int y = Mathf.RoundToInt(pos.y * ppu);

        Vector3 snapped = pos;
        snapped.x = (float)x / ppu;
        snapped.y = (float)y / ppu;

        state.PositionCorrection += (snapped - pos);
    }
}
