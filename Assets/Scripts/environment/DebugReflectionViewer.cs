using UnityEngine;

public class DebugReflectionViewer : MonoBehaviour
{
    public WaterReflectionCamera reflectionScript; // assign in inspector

    void OnGUI()
    {
        if (reflectionScript == null) {
            GUI.Label(new Rect(10,10,400,30), "Assign ReflectionCamera script in inspector");
            return;
        }

        // Try to fetch the RT via the material
        var mat = reflectionScript.waterMaterial;
        if (mat == null) {
            GUI.Label(new Rect(10,10,400,30), "Reflection script has no waterMaterial assigned");
            return;
        }

        var rt = mat.GetTexture("_ReflectionTex") as RenderTexture;
        if (rt == null) {
            GUI.Label(new Rect(10,10,400,30), "No RenderTexture found on material (_ReflectionTex missing)");
            return;
        }

        // Draw RenderTexture in the top-left (scaled)
        Rect r = new Rect(10, 40, 256, 128);
        GUI.DrawTexture(r, rt, ScaleMode.ScaleToFit, false);
        GUI.Label(new Rect(10, 10, 400, 20), $"RT: {rt.width}x{rt.height}  Format:{rt.format}");

        // Show some runtime info
        GUI.Label(new Rect(10, 180, 500, 20), $"WaterY: {reflectionScript.waterY:F2}  RealtimeUpdate: {reflectionScript.useRealtimeUpdate}");
    }
}
