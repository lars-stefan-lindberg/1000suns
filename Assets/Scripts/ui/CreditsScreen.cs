using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] private GameObject _collectibleCountLabel;

    void Awake() {
        TextMeshProUGUI collectiblesText = _collectibleCountLabel.GetComponent<TextMeshProUGUI>();
        collectiblesText.text = CollectibleManager.obj.GetNumberOfCollectiblesPicked() + " out of " + CollectibleManager.NUMBER_OF_COLLECTIBLES;
    }

    public void QuitToTitleScreen() {
        StartCoroutine(QuitToTitleScreenDelayed());
    }

    private IEnumerator QuitToTitleScreenDelayed() {
        yield return new WaitForSeconds(3f);
                
        PauseMenuManager.obj.Quit();
    }
}