using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterReflectionCamera : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Material waterMaterial;
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

    [Header("Options")]
    public bool useRealtimeUpdate = true;

    Camera reflectionCamera;
    RenderTexture reflectionTexture;

    int lastFrameRendered = -1;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateRenderTexture();
        CreateReflectionCamera();
        AssignToMaterial();
    }

    void OnDisable()
    {
        if (reflectionCamera) Destroy(reflectionCamera.gameObject);
        if (reflectionTexture) reflectionTexture.Release();
    }

    void Update()
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
        // 4. PASS WATERLINE TO SHADER
        //
        if (waterMaterial != null)
            waterMaterial.SetFloat("_WaterY", waterY);

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
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            waterMaterial = sr.sharedMaterial;

        if (mainCamera == null)
            mainCamera = Camera.main;
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
