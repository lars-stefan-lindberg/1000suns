using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;

public class BootGame : MonoBehaviour
{
    [SerializeField] private SceneField _commonObjects;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private SceneField _bootGame;

    void Start()
    {
        StartCoroutine(Boot());
    }

    private IEnumerator Boot() {
        AsyncOperation loadCommonOperation = SceneManager.LoadSceneAsync(_commonObjects, LoadSceneMode.Additive);
        while(!loadCommonOperation.isDone) {
            yield return null;
        }
        yield return LoadPreferredLanguage();
        
        GeneralTracker.obj.TrackGameStarted();

        AsyncOperation loadTitleScreenOperation = SceneManager.LoadSceneAsync(_titleScreen, LoadSceneMode.Additive);
        while(!loadTitleScreenOperation.isDone) {
            yield return null;
        }

        SceneManager.UnloadSceneAsync(_bootGame.SceneName);
    }

    private IEnumerator LoadPreferredLanguage() {
        yield return LocalizationSettings.InitializationOperation;
        
        string preferredLanguage = null;
        
        if(PlayerPrefs.HasKey("PreferredLanguage")) {
            preferredLanguage = PlayerPrefs.GetString("PreferredLanguage");
            //Debug.Log("player has preferred language: " + preferredLanguage);
        }         
        var locale = LocalizationSettings.AvailableLocales.GetLocale(preferredLanguage);

        if(locale == null) {
            locale = LocalizationSettings.AvailableLocales.GetLocale("en");
            if(locale == null && LocalizationSettings.AvailableLocales.Locales.Count > 0) {
                locale = LocalizationSettings.AvailableLocales.Locales[0];
            }
        }
        
        if(locale != null) {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}
