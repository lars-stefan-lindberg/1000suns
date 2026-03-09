using TMPro;
using UnityEngine;

public class InputIconTMP : MonoBehaviour
{
    public TMP_Text text;

    public void SetAction(string deviceLayoutName, string controlPath,
        string format = "<sprite name=\"{0}\" tint=1>")
    {
        var asset = InputIconManager.obj.GetSpriteAsset(deviceLayoutName);
        var iconName = InputIconManager.obj.GetIconName(controlPath);

        text.spriteAsset = asset;
        text.text = string.Format(format, iconName);
    }
}
