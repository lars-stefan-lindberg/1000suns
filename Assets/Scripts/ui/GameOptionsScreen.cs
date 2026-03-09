using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using System.Collections;

public class GameOptionsScreen : UIScreen
{
    public UnityEvent OnBack;
    [SerializeField] private LanguageSelectMenuItem _languageSelectMenuItem;

    public void OnLanguageChange() {
        if(_languageSelectMenuItem != null) {
            string languageIdentifier = _languageSelectMenuItem.GetCurrentLanguageIdentifier();
            if(!string.IsNullOrEmpty(languageIdentifier)) {
                StartCoroutine(ChangeLocale(languageIdentifier));
            }
        }
    }

    public void OnLanguageSelectorButtonClick() {
        UISoundPlayer.obj.PlaySliderTick();
        _languageSelectMenuItem.SelectNext();
    }

    private IEnumerator ChangeLocale(string localeIdentifier) {
        yield return LocalizationSettings.InitializationOperation;
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeIdentifier);
        if(locale != null) {
            LocalizationSettings.SelectedLocale = locale;
        } else {
            Debug.LogWarning($"Locale with identifier '{localeIdentifier}' not found");
        }
    }

    public void ResetToDefaults() {
        //TODO reset to "default" language (operating system language?)
    }

    public void Back() {
        UISoundPlayer.obj.PlayBack();
        OnBack?.Invoke();
    }
}
