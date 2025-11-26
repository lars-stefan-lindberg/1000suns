using UnityEngine;

public class WaterReflectionController : MonoBehaviour
{
    public Material waterMaterial;
    public Texture2D normalMap;

    [Header("Normal map animation")]
    public Vector2 scrollSpeed1 = new Vector2(0.02f, 0.04f);
    public Vector2 scrollSpeed2 = new Vector2(-0.03f, 0.02f);
    public float normalStrength = 0.02f;

    [Header("Fade & tint")]
    [Range(0,1)] public float reflectionOpacity = 0.7f;
    public Color waterTint = new Color(0.6f,0.75f,0.9f,1f);

    Vector2 offset1 = Vector2.zero;
    Vector2 offset2 = Vector2.zero;

    void Update()
    {
        if (waterMaterial == null) return;

        offset1 += scrollSpeed1 * Time.deltaTime;
        offset2 += scrollSpeed2 * Time.deltaTime;

        offset1 = new Vector2(
            Mathf.Repeat(offset1.x, 1000f),
            Mathf.Repeat(offset1.y, 1000f)
        );
        offset2 = new Vector2(
            Mathf.Repeat(offset2.x, 1000f),
            Mathf.Repeat(offset2.y, 1000f)
        );

        waterMaterial.SetTexture("_NormalTex", normalMap);
        waterMaterial.SetVector("_NormalOffset1", offset1);
        waterMaterial.SetVector("_NormalOffset2", offset2);
        waterMaterial.SetFloat("_NormalStrength", normalStrength);
        waterMaterial.SetFloat("_ReflectionOpacity", reflectionOpacity);
        waterMaterial.SetColor("_WaterTint", waterTint);
    }
}
