using TMPro;
using UnityEngine;

public class InputIconTMP : MonoBehaviour
{
    private const string DEFAULT_SPRITE_FORMAT = "<sprite name=\"{0}\" tint=1>";

    public TMP_Text text;

    /// <summary>
    /// Displays the icon for a binding. Keyboard and gamepad controls use a sprite from the matching
    /// sprite asset; mouse controls fall back to a short text placeholder (e.g. "LMB") because there are
    /// no mouse glyphs yet. Unbound controls clear the text.
    /// </summary>
    public void SetAction(string deviceLayoutName, string controlPath,
        string format = DEFAULT_SPRITE_FORMAT)
    {
        if (text == null || InputIconManager.obj == null)
            return;

        var iconManager = InputIconManager.obj;
        text.spriteAsset = iconManager.GetSpriteAsset(deviceLayoutName);
        text.text = iconManager.GetBindingDisplayMarkup(deviceLayoutName, controlPath, format);
    }
}
