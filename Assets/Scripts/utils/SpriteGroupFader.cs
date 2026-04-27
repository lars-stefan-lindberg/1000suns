using System.Collections;
using UnityEngine;

public class SpriteGroupFader : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] renderers;

    public void SetAlpha(float alpha)
    {
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
