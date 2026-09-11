using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterReflectionCamera : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public LayerMask reflectionMask = ~0;

    [Header("RenderTexture")]
    [Tooltip("RenderTexture size will be auto-calculated from main camera aspect if autoSize=true")]
    public bool autoSize = true;
    public int textureWidth = 1024;
    public int textureHeight = 512;
    [Tooltip("Scale factor on top of game resolution (1 = native, 2 = double)")]
    public int resolutionScale = 2;
    public RenderTextureFormat rtFormat = RenderTextureFormat.Default;

    [Header("Waterline")]
    public float waterY = 0f;
    [Tooltip("X position where the reflection is anchored (won't scroll horizontally)")]
    public float reflectionAnchorX = 0f;

    [Header("Options")]
    public bool useRealtimeUpdate = true;
    [Tooltip("Snap camera position to pixel grid to prevent sub-pixel jittering")]
    public bool snapCameraToPixelGrid = true;
    [Tooltip("Use hysteresis snapping to prevent back-and-forth pixel snapping")]
    public bool useHysteresisSnapping = true;
    [Tooltip("Hysteresis margin in pixels - prevents snapping back until this threshold is crossed")]
    public float snapHysteresisPixels = 0.1f;

    Camera reflectionCamera;
    RenderTexture reflectionTexture;
    PixelPerfectCamera pixelPerfectCamera;
    SpriteRenderer spriteRenderer;
    Material waterMaterial; // Material instance (auto-created by Unity when accessing spriteRenderer.material)

    int lastFrameRendered = -1;
    int cachedAssetsPixelsPerUnit;
    float cachedInversePixelsPerUnit;
    Vector2Int lastSnappedPixels;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
        {
            pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        }

        // Get material instance from sprite renderer (Unity auto-creates instance)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            waterMaterial = spriteRenderer.material;
        }

        CachePixelPerfectValues();
        CreateRenderTexture();
        CreateReflectionCamera();
        AssignToMaterial();
        
        // Initialize last snapped position
        if (mainCamera != null)
        {
            lastSnappedPixels = WorldToPixel(mainCamera.transform.position);
        }
    }

    void OnEnable()
    {
        // Re-enable reflection camera if it exists
        if (reflectionCamera != null)
        {
            reflectionCamera.enabled = true;
        }
    }

    void OnDisable()
    {
        // Just disable the reflection camera, don't destroy it
        if (reflectionCamera != null)
        {
            reflectionCamera.enabled = false;
        }
    }

    void OnDestroy()
    {
        // Clean up when the component is actually destroyed
        if (reflectionCamera != null)
        {
            Destroy(reflectionCamera.gameObject);
        }
        if (reflectionTexture != null)
        {
            reflectionTexture.Release();
        }
    }

    void LateUpdate()
    {
        if (useRealtimeUpdate)
        {
            // ensure we render only once per frame
            if (Time.frameCount != lastFrameRendered)
            {
                lastFrameRendered = Time.frameCount;
                UpdateReflection();
            }
        }
    }

    public Material GetWaterMaterial() {
        if (spriteRenderer != null)
        {
            return spriteRenderer.material;
        }
        return null;
    }

    public void UpdateReflection()
    {
        if (mainCamera == null || reflectionCamera == null)
            return;

        // Ensure RT exists (in case of resize)
        reflectionCamera.targetTexture = reflectionTexture;

        // Match core camera settings
        reflectionCamera.orthographic = mainCamera.orthographic;
        reflectionCamera.orthographicSize = mainCamera.orthographicSize;
        reflectionCamera.fieldOfView = mainCamera.fieldOfView;
        reflectionCamera.nearClipPlane = mainCamera.nearClipPlane;
        reflectionCamera.farClipPlane = mainCamera.farClipPlane;
        reflectionCamera.aspect = mainCamera.aspect;

        //
        // 1. MIRROR CAMERA POSITION ACROSS WATERLINE
        //
        Vector3 mainPos = mainCamera.transform.position;
        
        // Snap camera position to pixel grid if enabled
        if (snapCameraToPixelGrid)
        {
            if (useHysteresisSnapping)
            {
                mainPos = SnapToPixelGridHysteresis(mainPos, ref lastSnappedPixels);
            }
            else
            {
                mainPos = SnapToPixelGrid(mainPos);
            }
        }
        
        float dist = mainPos.y - waterY;
        reflectionCamera.transform.position = new Vector3(
            mainPos.x,
            waterY - dist,    // (waterY - (mainY - waterY)) = mirrored Y
            mainPos.z
        );

        //
        // 2. MIRROR THE VIEW USING A REFLECTION MATRIX
        //
        Vector4 plane = new Vector4(0, 1, 0, -waterY);
        Matrix4x4 reflectionMat = CalculateReflectionMatrix(plane);

        reflectionCamera.worldToCameraMatrix =
            mainCamera.worldToCameraMatrix * reflectionMat;

        //
        // 3. MIRROR THE PROJECTION (vertical flip)
        //
        Matrix4x4 proj = mainCamera.projectionMatrix;
        proj.m11 = -proj.m11;
        reflectionCamera.projectionMatrix = proj;

        //
        // 4. PASS WATERLINE AND CAMERA PARAMS TO SHADER
        //
        if (waterMaterial != null)
        {
            waterMaterial.SetFloat("_WaterY", waterY);
            waterMaterial.SetFloat("_CameraOrthoSize", mainCamera.orthographicSize);
            waterMaterial.SetFloat("_CameraAspect", mainCamera.aspect);
            waterMaterial.SetFloat("_ReflectionAnchorX", reflectionAnchorX);
            waterMaterial.SetFloat("_CameraPosX", mainPos.x);
            waterMaterial.SetFloat("_CameraPosY", mainPos.y);
        }

        //
        // 5. RENDER
        //
        reflectionCamera.cullingMask = reflectionMask;
        reflectionCamera.Render();
    }

    // public void UpdateReflection()
    // {
    //     if (mainCamera == null || reflectionCamera == null) return;

    //     // TEMP: no mirroring, no flipping, no reflection-matrix.
    //     reflectionCamera.transform.position = mainCamera.transform.position;
    //     reflectionCamera.transform.rotation = mainCamera.transform.rotation;

    //     reflectionCamera.orthographic = mainCamera.orthographic;
    //     reflectionCamera.orthographicSize = mainCamera.orthographicSize;
    //     reflectionCamera.nearClipPlane = mainCamera.nearClipPlane;
    //     reflectionCamera.farClipPlane = mainCamera.farClipPlane;
    //     reflectionCamera.fieldOfView = mainCamera.fieldOfView;
    //     reflectionCamera.aspect = mainCamera.aspect;

    //     reflectionCamera.targetTexture = reflectionTexture;
    //     reflectionCamera.cullingMask = reflectionMask;

    //     reflectionCamera.Render();
    // }

    void EnsureRenderTextureSize()
    {
        if (!autoSize) return;

        // Try to size the RenderTexture according to main camera pixel dimensions * resolutionScale
        int desiredWidth = Mathf.Max(1, Screen.width * resolutionScale);
        int desiredHeight = Mathf.Max(1, Screen.height * resolutionScale);

        // Keep aspect close to main camera's aspect (in case Screen.* is different)
        float camAspect = mainCamera.aspect;
        if ((float)desiredWidth / desiredHeight < camAspect)
            desiredWidth = Mathf.RoundToInt(desiredHeight * camAspect);
        else
            desiredHeight = Mathf.RoundToInt(desiredWidth / camAspect);

        if (reflectionTexture == null || reflectionTexture.width != desiredWidth || reflectionTexture.height != desiredHeight)
        {
            // recreate RT
            if (reflectionTexture != null) reflectionTexture.Release();
            reflectionTexture = new RenderTexture(desiredWidth, desiredHeight, 16, rtFormat);
            reflectionTexture.name = "ReflectionRT_" + gameObject.name;
            reflectionTexture.wrapMode = TextureWrapMode.Repeat;
            reflectionTexture.filterMode = FilterMode.Bilinear;
            if (waterMaterial != null) waterMaterial.SetTexture("_ReflectionTex", reflectionTexture);

            // set PixelSize for shader
            if (waterMaterial != null)
            {
                float pixelSize = 1.0f / (float)reflectionTexture.height;
                waterMaterial.SetFloat("_PixelSize", pixelSize);
            }

            // assign to reflectionCamera if exists
            if (reflectionCamera != null) reflectionCamera.targetTexture = reflectionTexture;
        }
    }

    void CreateRenderTexture()
    {
        if (autoSize)
        {
            // placeholder; real RT created in EnsureRenderTextureSize
            reflectionTexture = null;
            return;
        }

        reflectionTexture = new RenderTexture(textureWidth, textureHeight, 16, rtFormat);
        reflectionTexture.name = "ReflectionRT_" + gameObject.name;
        reflectionTexture.wrapMode = TextureWrapMode.Repeat;
        reflectionTexture.filterMode = FilterMode.Bilinear;
    }

    void CreateReflectionCamera()
    {
        GameObject go = new GameObject("ReflectionCamera_" + gameObject.name);
        go.hideFlags = HideFlags.HideAndDontSave;
        reflectionCamera = go.AddComponent<Camera>();
        reflectionCamera.enabled = false;
        reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
        reflectionCamera.backgroundColor = Color.clear;
        reflectionCamera.cullingMask = reflectionMask;
        reflectionCamera.allowMSAA = false;
        reflectionCamera.usePhysicalProperties = false;

        // If we are auto-sizing, create RT now
        EnsureRenderTextureSize();
    }

    void AssignToMaterial()
    {
        if (waterMaterial != null && reflectionTexture != null)
        {
            waterMaterial.SetTexture("_ReflectionTex", reflectionTexture);
            waterMaterial.SetFloat("_WaterY", waterY);
            waterMaterial.SetFloat("_PixelSize", 1.0f / (float)reflectionTexture.height);
        }
    }

    void Reset()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void CachePixelPerfectValues()
    {
        cachedAssetsPixelsPerUnit = GetAssetsPixelsPerUnit();
        cachedInversePixelsPerUnit = 1f / cachedAssetsPixelsPerUnit;
    }

    private int GetAssetsPixelsPerUnit()
    {
        if (pixelPerfectCamera != null && pixelPerfectCamera.assetsPPU > 0)
        {
            return pixelPerfectCamera.assetsPPU;
        }

        return 16;
    }

    private Vector2Int WorldToPixel(Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x * cachedAssetsPixelsPerUnit),
            Mathf.RoundToInt(position.y * cachedAssetsPixelsPerUnit)
        );
    }

    private Vector3 PixelToWorld(Vector2Int pixels, float z)
    {
        return new Vector3(pixels.x * cachedInversePixelsPerUnit, pixels.y * cachedInversePixelsPerUnit, z);
    }

    private Vector3 SnapToPixelGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x * cachedAssetsPixelsPerUnit);
        int y = Mathf.RoundToInt(position.y * cachedAssetsPixelsPerUnit);

        position.x = x * cachedInversePixelsPerUnit;
        position.y = y * cachedInversePixelsPerUnit;
        return position;
    }

    private Vector3 SnapToPixelGridHysteresis(Vector3 position, ref Vector2Int lastSnappedPixels)
    {
        float xPixels = position.x * cachedAssetsPixelsPerUnit;
        float yPixels = position.y * cachedAssetsPixelsPerUnit;

        float margin = Mathf.Max(0f, snapHysteresisPixels);

        // Only snap to a new pixel if we've moved past the threshold with margin
        // This prevents back-and-forth snapping when moving in one direction
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

    private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = 1F - 2F * plane[0] * plane[0];
        m.m01 = -2F * plane[0] * plane[1];
        m.m02 = -2F * plane[0] * plane[2];
        m.m03 = -2F * plane[3] * plane[0];

        m.m10 = -2F * plane[1] * plane[0];
        m.m11 = 1F - 2F * plane[1] * plane[1];
        m.m12 = -2F * plane[1] * plane[2];
        m.m13 = -2F * plane[3] * plane[1];

        m.m20 = -2F * plane[2] * plane[0];
        m.m21 = -2F * plane[2] * plane[1];
        m.m22 = 1F - 2F * plane[2] * plane[2];
        m.m23 = -2F * plane[3] * plane[2];

        return m;
    }
}
