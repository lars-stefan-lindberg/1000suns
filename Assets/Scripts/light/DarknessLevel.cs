using System;
using System.Collections.Generic;
using UnityEngine;

public enum DarknessLevelType
{
    PitchBlack,
    VeryDark,
    Dark,
    MediumDark,
    LightDark
}

[Serializable]
public class DarknessLevelData
{
    public DarknessLevelType level;
    [Range(0f, 1f)] public float hue;
    [Range(0f, 1f)] public float saturation;
    [Range(0f, 1f)] public float value;
}

public class DarknessLevel : MonoBehaviour
{
    [SerializeField] private List<DarknessLevelData> _darknessLevels = new List<DarknessLevelData>
    {
        new DarknessLevelData { level = DarknessLevelType.PitchBlack, hue = 0f, saturation = 0f, value = 0f },
        new DarknessLevelData { level = DarknessLevelType.VeryDark, hue = 0f, saturation = 0f, value = 0.01f },
        new DarknessLevelData { level = DarknessLevelType.Dark, hue = 0f, saturation = 0f, value = 0.05f },
        new DarknessLevelData { level = DarknessLevelType.MediumDark, hue = 0f, saturation = 0f, value = 0.8f },
        new DarknessLevelData { level = DarknessLevelType.LightDark, hue = 0f, saturation = 0f, value = 0.15f }
    };

    private Dictionary<DarknessLevelType, Color> _levelColorMap;

    private void Awake()
    {
        BuildColorMap();
    }

    private void BuildColorMap()
    {
        _levelColorMap = new Dictionary<DarknessLevelType, Color>();
        foreach (var levelData in _darknessLevels)
        {
            Color color = Color.HSVToRGB(levelData.hue, levelData.saturation, levelData.value);
            _levelColorMap[levelData.level] = color;
        }
    }

    public Color GetColorForLevel(DarknessLevelType level)
    {
        if (_levelColorMap == null)
        {
            BuildColorMap();
        }

        if (_levelColorMap.TryGetValue(level, out Color color))
        {
            return color;
        }

        Debug.LogWarning($"DarknessLevel: No color found for level {level}, returning default color.");
        return Color.gray;
    }
}
