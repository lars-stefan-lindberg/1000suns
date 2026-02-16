using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageTest : MonoBehaviour
{
    [ContextMenu("Set Swedish")]
    public void SetLanguage()
    {
        var index = 6;
        Debug.Log("locale: " + LocalizationSettings.SelectedLocale);
        StartCoroutine(ChangeLocale(index));
    }

    IEnumerator ChangeLocale(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale =
            LocalizationSettings.AvailableLocales.Locales[index];
        Debug.Log("locale: " + LocalizationSettings.SelectedLocale);
    }
}
